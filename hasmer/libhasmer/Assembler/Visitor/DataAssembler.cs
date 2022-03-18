using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Hasmer.Assembler.Parser;

namespace Hasmer.Assembler.Visitor {
    /// <summary>
    /// Represents a data buffer as it is declared in a Hasm file.
    /// </summary>
    public class HasmAssemblerDataBuffer {
        /// <summary>
        /// The raw bytes of the buffer.
        /// </summary>
        public List<byte> RawBuffer { get; set; }

        /// <summary>
        /// Represents key-value between the label in the buffer (i.e. A5, key = 5) and the offset of that label in the <see cref="RawBuffer"/>.
        /// </summary>
        public Dictionary<uint, int> HasmBuffer { get; set; }

        public HasmAssemblerDataBuffer() {
            RawBuffer = new List<byte>();
            HasmBuffer = new Dictionary<uint, int>();
        }
    }

    /// <summary>
    /// Assembles the declared data tokens (i.e. ".data") for creating the array value and object key/value buffers.
    /// </summary>
    public class DataAssembler {
        /// <summary>
        /// The token stream of the Hasm file.
        /// </summary>
        private HasmTokenStream Stream;

        /// <summary>
        /// The array buffer.
        /// </summary>
        public HasmAssemblerDataBuffer ArrayBuffer { get; set; }

        /// <summary>
        /// The object key buffer.
        /// </summary>
        public HasmAssemblerDataBuffer ObjectKeyBuffer { get; set; }

        /// <summary>
        /// The object value buffer.
        /// </summary>
        public HasmAssemblerDataBuffer ObjectValueBuffer { get; set; }

        /// <summary>
        /// Represents the string table.
        /// The key is the string and the value is the ID of the string.
        /// The ID is a sequential value, incremented for each new string.
        /// </summary>
        public Dictionary<string, uint> StringTable { get; set; }

        /// <summary>
        /// Creates a new data assembler.
        /// </summary>
        public DataAssembler(HasmTokenStream stream) {
            Stream = stream;
            StringTable = new Dictionary<string, uint>();
            ArrayBuffer = new HasmAssemblerDataBuffer();
            ObjectKeyBuffer = new HasmAssemblerDataBuffer();
            ObjectValueBuffer = new HasmAssemblerDataBuffer();
        }

        /// <summary>
        /// Gets a <see cref="HasmAssemblerDataBuffer"/> by its label (i.e. "A", "K", or "V").
        /// </summary>
        private HasmAssemblerDataBuffer GetBufferByName(LabelType type) {
            return type switch {
                LabelType.ArrayBuffer => ArrayBuffer,
                LabelType.ObjectKeyBuffer => ObjectKeyBuffer,
                LabelType.ObjectValueBuffer => ObjectValueBuffer,
                _ => throw new Exception("invalid buffer label: " + type)
            };
        }

        /// <summary>
        /// Returns the ID of the given string in the <see cref="StringTable"/>.
        /// If the string is not already present in the string table,
        /// it is added and the ID of the newly added string is returned.
        /// </summary>
        public uint GetStringId(string str) {
            if (StringTable.ContainsKey(str)) {
                return StringTable[str];
            }

            uint newId = (uint)StringTable.Count;
            StringTable[str] = newId;
            return newId;
        }

        /// <summary>
        /// Parses all the data tokens and assembles them into data.
        /// </summary>
        public void Assemble() {
            foreach (HasmToken token in Stream.ReadTokens()) {
                if (token is HasmDataDeclarationToken data) {
                    HasmDataDeclarationType type = data.DataType;
                    uint labelIndex = data.Label.LabelIndex.GetValueAsUInt32();

                    HasmAssemblerDataBuffer buffer = GetBufferByName(data.Label.LabelType);
                    buffer.HasmBuffer[labelIndex] = buffer.RawBuffer.Count;

                    HbcDataBufferTagType tagType = type switch {
                        HasmDataDeclarationType.Null => HbcDataBufferTagType.Null,
                        HasmDataDeclarationType.True => HbcDataBufferTagType.True,
                        HasmDataDeclarationType.False => HbcDataBufferTagType.False,
                        HasmDataDeclarationType.String => HbcDataBufferTagType.ByteString, // this gets overwritten depending on the string offsets
                        HasmDataDeclarationType.Number => HbcDataBufferTagType.Number,
                        HasmDataDeclarationType.Integer => HbcDataBufferTagType.Integer,
                        _ => throw new Exception("invalid declaration type")
                    };

                    if (type == HasmDataDeclarationType.String) {
                        List<uint> ids = data.Data.Cast<HasmStringToken>().Select(str => GetStringId(str.Value)).ToList();
                        foreach (uint id in ids) {
                            if (id > ushort.MaxValue) {
                                tagType = HbcDataBufferTagType.LongString;
                                break;
                            } else if (id > byte.MaxValue) {
                                tagType = HbcDataBufferTagType.ShortString;
                            }
                        }
                    }

                    using MemoryStream ms = new MemoryStream();
                    using BinaryWriter bw = new BinaryWriter(ms);

                    const byte TAG_MASK = 0x70;
                    if (data.Data.Count > 0x0F) {
                        byte keyTag = (byte)(((byte)tagType | (byte)(data.Data.Count >> 8) | 0x80) & TAG_MASK);
                        bw.Write(keyTag);
                        bw.Write((byte) (data.Data.Count & 0xFF));
                    } else {
                        byte keyTag = (byte)(((byte)tagType | (byte)data.Data.Count) & TAG_MASK);
                        bw.Write(keyTag);
                    }

                    foreach (HasmLiteralToken literal in data.Data) {
                        if (literal is HasmIntegerToken integer) {
                            bw.Write(integer.GetValueAsInt32());
                        } else if (literal is HasmNumberToken number) {
                            bw.Write(number.Value);
                        } else if (literal is HasmStringToken str) {
                            uint id = GetStringId(str.Value);
                            if (tagType == HbcDataBufferTagType.ByteString) {
                                bw.Write((byte)id);
                            } else if (tagType == HbcDataBufferTagType.ShortString) {
                                bw.Write((ushort)id);
                            } else if (tagType == HbcDataBufferTagType.LongString) {
                                bw.Write(id);
                            } else {
                                throw new Exception("invalid tag type");
                            }
                        }

                        // simple values (true, false, null) are implicit and don't need to be writen
                        // the length of the data represents the amount of constant values in the buffer
                        // and since those values are constant they don't need to be written
                    }

                    byte[] dataArray = ms.ToArray();
                    buffer.RawBuffer.AddRange(dataArray);
                }
            }
        }
    }
}
