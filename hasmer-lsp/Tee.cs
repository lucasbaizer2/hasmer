using System;
using System.Diagnostics;
using System.IO;

namespace Hasmer.LSP {
    class Tee : Stream {
        [Flags]
        public enum StreamOwnership {
            OwnNone = 0x0,
            OwnPrimaryStream = 0x1,
            OwnSlaveStream = 0x2,
            OwnBoth = OwnPrimaryStream | OwnSlaveStream
        }
        public enum SlaveFailAction {
            Propogate,
            Ignore,
            Filter
        }

        public delegate SlaveFailAction SlaveFailHandler(
            object oSender, SlaveFailMethod method, Exception exc
        );

        public enum SlaveFailMethod {
            Read,
            Write,
            Seek
        }

        private readonly Stream m_primaryStream;
        private readonly Stream m_slaveStream;
        private StreamOwnership m_streamsOwned;

        private SlaveFailAction m_readFailAction = SlaveFailAction.Propogate;
        private SlaveFailAction m_writeFailAction = SlaveFailAction.Propogate;
        private SlaveFailAction m_seekFailAction = SlaveFailAction.Propogate;

        private SlaveFailHandler m_slaveReadFailFilter;
        private SlaveFailHandler m_slaveWriteFailFilter;
        private SlaveFailHandler m_slaveSeekFailFilter;

        private int m_lastReadResult;
        public Tee(
            Stream primaryStream, Stream slaveStream,
            StreamOwnership streamsOwned) {
            m_primaryStream = primaryStream;
            m_slaveStream = slaveStream;
            m_streamsOwned = streamsOwned;
        }
        public override void Close() {
            Flush();
            if ((m_streamsOwned & StreamOwnership.OwnPrimaryStream) > 0) {
                m_primaryStream.Close();
            }

            if ((m_streamsOwned & StreamOwnership.OwnSlaveStream) > 0) {
                m_slaveStream.Close();
            }
            base.Close();
        }
        public StreamOwnership StreamsOwned {
            get => m_streamsOwned;
            set => m_streamsOwned = value;
        }

        public Stream PrimaryStream => m_primaryStream;
        public Stream SlaveStream => m_slaveStream;
        public int LastReadResult => m_lastReadResult;
        public SlaveFailAction SlaveFailActions {
            set {
                SlaveReadFailAction = value;
                SlaveWriteFailAction = value;
                SlaveSeekFailAction = value;
            }
        }

        public SlaveFailHandler SlaveFailFilters {
            set {
                SlaveReadFailFilter = value;
                SlaveWriteFailFilter = value;
                SlaveSeekFailFilter = value;
            }
        }

        public SlaveFailAction SlaveReadFailAction {
            get => m_readFailAction;

            set {
                if (value == SlaveFailAction.Filter) {
                    throw new InvalidOperationException(
                        "You cannot set this property to "
                        + "SlaveFailAction.Filter manually.  Use the "
                        + "SlaveReadFailFilter property instead."
                    );
                } else {
                    m_slaveReadFailFilter = null;
                    m_readFailAction = value;
                }
            }
        }

        public SlaveFailAction SlaveWriteFailAction {
            get => m_writeFailAction;

            set {
                if (value == SlaveFailAction.Filter) {
                    throw new InvalidOperationException(
                        "You cannot set this property to "
                        + "SlaveFailAction.Filter manually.  Use the "
                        + "SlaveWriteFailFilter property instead."
                        );
                } else {
                    m_slaveWriteFailFilter = null;
                    m_writeFailAction = value;
                }
            }
        }

        public SlaveFailAction SlaveSeekFailAction {
            get => m_seekFailAction;

            set {
                if (value == SlaveFailAction.Filter) {
                    throw new InvalidOperationException(
                        "You cannot set this property to "
                        + "SlaveFailAction.Filter manually.  Use the "
                        + "SlaveSeekFailFilter property instead."
                        );
                } else {
                    m_slaveSeekFailFilter = null;
                    m_seekFailAction = value;
                }
            }
        }

        public SlaveFailHandler SlaveWriteFailFilter {
            get => m_slaveWriteFailFilter;

            set {
                if (m_slaveWriteFailFilter != null) {
                    m_writeFailAction = SlaveFailAction.Propogate;
                }

                m_slaveWriteFailFilter = value;
                if (value != null) {
                    m_writeFailAction = SlaveFailAction.Filter;
                }
            }
        }

        public SlaveFailHandler SlaveReadFailFilter {
            get => m_slaveReadFailFilter;

            set {
                if (m_slaveReadFailFilter != null) {
                    m_readFailAction = SlaveFailAction.Propogate;
                }

                m_slaveReadFailFilter = value;

                if (value != null) {
                    m_readFailAction = SlaveFailAction.Filter;
                }
            }
        }

        public SlaveFailHandler SlaveSeekFailFilter {
            get => m_slaveSeekFailFilter;

            set {
                if (m_slaveSeekFailFilter != null) {
                    m_seekFailAction = SlaveFailAction.Propogate;
                }

                m_slaveSeekFailFilter = value;
                if (value != null) {
                    m_seekFailAction = SlaveFailAction.Filter;
                }
            }
        }

        public override bool CanRead => m_primaryStream.CanRead;

        public override bool CanSeek => m_primaryStream.CanSeek && m_slaveStream.CanSeek;

        public override bool CanWrite => m_primaryStream.CanWrite && m_slaveStream.CanWrite;

        public override void Flush() {
            m_primaryStream.Flush();

            if (m_writeFailAction == SlaveFailAction.Propogate) {
                m_slaveStream.Flush();
            } else {
                try {
                    m_slaveStream.Flush();
                } catch (Exception exc) {
                    HandleSlaveException(
                        exc, SlaveFailMethod.Write,
                        m_writeFailAction
                    );
                }
            }
        }

        public override long Length => m_primaryStream.Length;

        public override void SetLength(long len) {
            long diff = len - m_primaryStream.Length;

            m_primaryStream.SetLength(len);

            if (m_seekFailAction == SlaveFailAction.Propogate) {
                m_slaveStream.SetLength(m_slaveStream.Length + diff);
            } else {
                try {
                    m_slaveStream.SetLength(m_slaveStream.Length + diff);
                } catch (Exception exc) {
                    HandleSlaveException(
                        exc, SlaveFailMethod.Seek, m_seekFailAction
                    );
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            m_lastReadResult = m_primaryStream.Read(buffer, offset, count);
            if (m_lastReadResult != 0) {
                if (m_readFailAction == SlaveFailAction.Propogate) {
                    m_slaveStream.Write(buffer, offset, m_lastReadResult);
                } else {
                    try {
                        m_slaveStream.Write(buffer, offset, m_lastReadResult);
                    } catch (Exception exc) {
                        HandleSlaveException(
                            exc, SlaveFailMethod.Read, m_readFailAction
                        );
                    }
                }
            }

            return m_lastReadResult;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            m_primaryStream.Write(buffer, offset, count);

            if (m_writeFailAction == SlaveFailAction.Propogate) {
                m_slaveStream.Write(buffer, offset, count);
            } else {
                try {
                    m_slaveStream.Write(buffer, offset, count);
                } catch (Exception exc) {
                    HandleSlaveException(
                        exc, SlaveFailMethod.Write,
                        m_writeFailAction
                    );
                }
            }
        }

        public override long Position {
            get => m_primaryStream.Position;

            set {
                long diff = value - m_primaryStream.Position;

                m_primaryStream.Position = value;

                if (m_seekFailAction == SlaveFailAction.Propogate) {
                    m_slaveStream.Position += diff;
                } else {
                    try {
                        m_slaveStream.Position += diff;
                    } catch (Exception exc) {
                        HandleSlaveException(
                            exc, SlaveFailMethod.Seek, m_seekFailAction
                        );
                    }
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (origin == SeekOrigin.Begin) {
                Position = offset;
            } else if (origin == SeekOrigin.Current) {
                Position += offset;
            } else if (origin == SeekOrigin.End) {
                Position = Length + offset;
            }

            return Position;
        }

        private void FilterException(Exception exc, SlaveFailMethod method) {
            SlaveFailAction action = SlaveFailAction.Filter;

            if (method == SlaveFailMethod.Read) {
                action = m_slaveReadFailFilter(this, method, exc);
            } else if (method == SlaveFailMethod.Write) {
                action = m_slaveWriteFailFilter(this, method, exc);
            } else if (method == SlaveFailMethod.Seek) {
                action = m_slaveSeekFailFilter(this, method, exc);
            } else {
                Debug.Assert(false, "Unhandled SlaveFailMethod.");
            }

            if (action == SlaveFailAction.Filter) {
                throw new InvalidOperationException(
                    "SlaveFailAction.Filter is not a valid return "
                    + "value for the ReadFailFilter delegate.",
                    exc
                );
            }
            HandleSlaveException(exc, method, action);
        }

        private void HandleSlaveException(
            Exception exc, SlaveFailMethod method, SlaveFailAction action) {
            if (action == SlaveFailAction.Propogate) {
                throw exc;
            } else if (action == SlaveFailAction.Ignore) {
                // Intentionally Empty
            } else if (action == SlaveFailAction.Filter) {
                FilterException(exc, method);
            } else {
                Debug.Assert(false, "Unhandled SlaveFailAction");
            }
        }
    }
}
