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
        public MemoryStream RawBuffer { get; set; }

        /// <summary>
        /// A writer for <see cref="RawBuffer" />.
        /// </summary>
        public BinaryWriter BufferWriter { get; set; }

        /// <summary>
        /// Represents key-value between the label in the buffer (i.e. A5, key = 5) and the offset of that label in the <see cref="RawBuffer"/>.
        /// </summary>
        public Dictionary<uint, long> HasmBuffer { get; set; }

        public HasmAssemblerDataBuffer() {
            RawBuffer = new MemoryStream();
            BufferWriter = new BinaryWriter(RawBuffer);
            HasmBuffer = new Dictionary<uint, long>();
        }
    }

    /// <summary>
    /// Assembles the declared data tokens (i.e. ".data") for creating the array value and object key/value buffers.
    /// </summary>
    public class DataAssembler {
        /// <summary>
        /// The tokens of the Hasm program.
        /// </summary>
        private HasmProgram Program;

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
        /// The string table, where each index is the entry's ID.
        /// </summary>
        public List<StringTableEntry> StringTable { get; set; }

        /// <summary>
        /// Fast lookup into the string table by the string's value.
        /// The key is the string and the value is the ID of the string.
        /// That is, the index into <see cref="StringTable" />.
        /// </summary>
        private Dictionary<string, uint> StringTableLookup { get; set; }

        /// <summary>
        /// Creates a new data assembler.
        /// </summary>
        public DataAssembler(HasmProgram program) {
            Program = program;
            StringTable = new List<StringTableEntry>();
            StringTableLookup = new Dictionary<string, uint>();
            ArrayBuffer = new HasmAssemblerDataBuffer();
            ObjectKeyBuffer = new HasmAssemblerDataBuffer();
            ObjectValueBuffer = new HasmAssemblerDataBuffer();
        }

        /// <summary>
        /// Gets a <see cref="HasmAssemblerDataBuffer"/> by its label (i.e. "A", "K", or "V").
        /// </summary>
        private HasmAssemblerDataBuffer GetBufferByName(HasmLabelKind kind) {
            return kind switch {
                HasmLabelKind.ArrayBuffer => ArrayBuffer,
                HasmLabelKind.ObjectKeyBuffer => ObjectKeyBuffer,
                HasmLabelKind.ObjectValueBuffer => ObjectValueBuffer,
                _ => throw new Exception($"invalid buffer label '{kind}'")
            };
        }

        /// <summary>
        /// Returns the ID of the given string in the <see cref="StringTable"/>.
        /// If the string is not already present in the string table,
        /// it is added and the ID of the newly added string is returned.
        /// </summary>
        public uint GetStringId(string s, StringKind kind) {
            uint id;
            if (StringTableLookup.TryGetValue(s, out id)) {
                return id;
            }

            id = (uint)StringTable.Count;
            bool isUTF16 = !s.All(char.IsAscii);
            StringTable.Add(new StringTableEntry(kind, s, isUTF16));
            StringTableLookup[s] = id;

            return id;
        }

        /// <summary>
        /// Parses all the data tokens and assembles them into data.
        /// </summary>
        public void Assemble() {
            foreach (HasmDataDeclaration data in Program.Data) {
                HasmDataDeclarationKind kind = data.Kind;
                uint labelIndex = data.Label.Index.GetValueAsUInt32();

                HasmAssemblerDataBuffer buffer = GetBufferByName(data.Label.Kind);
                buffer.HasmBuffer[labelIndex] = buffer.RawBuffer.Position;

                HbcDataBufferTagType tagType = kind switch {
                    HasmDataDeclarationKind.Null => HbcDataBufferTagType.Null,
                    HasmDataDeclarationKind.True => HbcDataBufferTagType.True,
                    HasmDataDeclarationKind.False => HbcDataBufferTagType.False,
                    HasmDataDeclarationKind.String => HbcDataBufferTagType.ByteString, // this gets overwritten depending on the string offsets
                    HasmDataDeclarationKind.Number => HbcDataBufferTagType.Number,
                    HasmDataDeclarationKind.Integer => HbcDataBufferTagType.Integer,
                    _ => throw new Exception("invalid declaration type")
                };

                if (kind == HasmDataDeclarationKind.String) {
                    IEnumerable<uint> ids = data.Elements.Cast<HasmStringToken>().Select(str => GetStringId(str.Value, StringKind.Literal));
                    foreach (uint id in ids) {
                        if (id > ushort.MaxValue) {
                            tagType = HbcDataBufferTagType.LongString;
                            break;
                        } else if (id > byte.MaxValue) {
                            tagType = HbcDataBufferTagType.ShortString;
                        }
                    }
                }

                const byte TAG_MASK = 0x70;
                if (data.Count > 0x0F) {
                    byte keyTag = (byte)(((byte)tagType | (byte)(data.Count >> 8) | 0x80) & TAG_MASK);
                    buffer.BufferWriter.Write(keyTag);
                    buffer.BufferWriter.Write((byte)(data.Count & 0xFF));
                } else {
                    byte keyTag = (byte)(((byte)tagType | (byte)data.Count) & TAG_MASK);
                    buffer.BufferWriter.Write(keyTag);
                }

                if (data.Elements != null) {
                    foreach (HasmLiteralToken literal in data.Elements) {
                        uint id = uint.MaxValue;
                        if (literal is HasmIntegerToken integer) {
                            buffer.BufferWriter.Write(integer.GetValueAsInt32());
                            continue;
                        } else if (literal is HasmNumberToken number) {
                            buffer.BufferWriter.Write(number.Value);
                            continue;
                        } else if (literal is HasmStringToken str) {
                            id = GetStringId(str.Value, StringKind.Literal);
                        } else if (literal is HasmIdentifierToken ident) {
                            id = GetStringId(ident.Value, StringKind.Identifier);
                        }

                        if (id != uint.MaxValue) {
                            if (tagType == HbcDataBufferTagType.ByteString) {
                                buffer.BufferWriter.Write((byte)id);
                            } else if (tagType == HbcDataBufferTagType.ShortString) {
                                buffer.BufferWriter.Write((ushort)id);
                            } else if (tagType == HbcDataBufferTagType.LongString) {
                                buffer.BufferWriter.Write(id);
                            } else {
                                throw new Exception("invalid tag type");
                            }
                        }

                        // simple values (true, false, null) are implicit and don't need to be writen
                        // the length of the data represents the amount of constant values in the buffer
                        // and since those values are constant they don't need to be written
                    }
                }
            }
        }
    }
}
