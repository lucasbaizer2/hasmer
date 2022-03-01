using System;
using System.IO;
using System.Text;

namespace Hasmer.LSP {
    public class Dup : Stream {
        private string Name;

        public Dup(string name) {
            Name = name;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush() {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            DateTime now = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Raw message from " + Name + " " + now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            byte[] truncatedArray = new byte[count];
            for (int i = offset; i < offset + count; i++) {
                truncatedArray[i - offset] = buffer[i];
            }
            string str = Encoding.UTF8.GetString(truncatedArray);
            sb.AppendLine("data (length " + str.Length + ")= '" + str + "'");
            Console.Error.WriteLine(sb.ToString());
        }
    }
}
