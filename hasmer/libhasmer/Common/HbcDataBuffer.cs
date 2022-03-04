using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hasmer {
    /// <summary>
    /// Represents the type of a series of data in the DataBuffer.
    /// </summary>
    public enum HbcDataBufferTagType {
        Null = 0,
        True = 1 << 4,
        False = 2 << 4,
        Number = 3 << 4,
        LongString = 4 << 4,
        ShortString = 5 << 4,
        ByteString = 6 << 4,
        Integer = 7 << 4
    }

    /// <summary>
    /// Represents the header of an array in the data buffer.
    /// </summary>
    public class HbcDataBufferPrefix {
        /// <summary>
        /// The amount of values represented by the array.
        /// </summary>
        public uint Length { get; set; }
        /// <summary>
        /// The type of the data in the data buffer.
        /// </summary>
        public HbcDataBufferTagType TagType { get; set; }
    }

    /// <summary>
    /// Represents an entry in the data buffer (data type and subsequent items).
    /// </summary>
    public class HbcDataBufferItems {
        /// <summary>
        /// The header for the data buffer entry.
        /// </summary>
        public HbcDataBufferPrefix Prefix { get; set; }
        /// <summary>
        /// The items for the data buffer entry.
        /// </summary>
        public PrimitiveValue[] Items { get; set; }
        /// <summary>
        /// The offset of the entry (from the start of the header) relative to the start of the entire buffer.
        /// </summary>
        public uint Offset { get; set; }
    }

    /// <summary>
    /// Represents a Hermes data buffer, such as the array buffer.
    /// </summary>
    public class HbcDataBuffer {
        /// <summary>
        /// The raw binary of the buffer.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Creates a new HbcDataBuffer given the raw binary data in the buffer.
        /// </summary>
        public HbcDataBuffer(byte[] buffer) {
            Buffer = buffer;
        }

        /// <summary>
        /// Reads the entire buffer and disassembles it into HbcDataBufferItems objects for each entry in the buffer.
        /// </summary>
        public List<HbcDataBufferItems> ReadAll(HbcFile source) {
            using MemoryStream ms = new MemoryStream(Buffer);
            using BinaryReader reader = new BinaryReader(ms);

            List<HbcDataBufferItems> itemsList = new List<HbcDataBufferItems>();
            while (ms.Position < ms.Length) {
                uint offset = (uint)ms.Position;
                HbcDataBufferPrefix prefix = ReadTagType(reader);
                PrimitiveValue[] values = new PrimitiveValue[prefix.Length];
                for (int i = 0; i < values.Length; i++) {
                    values[i] = ReadValue(source, prefix.TagType, reader);
                }
                itemsList.Add(new HbcDataBufferItems {
                    Prefix = prefix,
                    Items = values,
                    Offset = offset
                });
            }

            return itemsList;
        }

        /// <summary>
        /// Disassembles a single HbcDataBufferItems from an offset in the data buffer (i.e. from an instruction operand).
        /// </summary>
        public HbcDataBufferItems Read(HbcFile source, uint offset) {
            using MemoryStream ms = new MemoryStream(Buffer);
            using BinaryReader reader = new BinaryReader(ms);
            ms.Position = offset;

            HbcDataBufferPrefix prefix = ReadTagType(reader);
            PrimitiveValue[] values = new PrimitiveValue[prefix.Length];
            for (int i = 0; i < values.Length; i++) {
                values[i] = ReadValue(source, prefix.TagType, reader);
            }

            return new HbcDataBufferItems {
                Prefix = prefix,
                Items = values
            };
        }

        /// <summary>
        /// Reads a single PrimitiveValue from a stream given the type of the value.
        /// </summary>
        private PrimitiveValue ReadValue(HbcFile source, HbcDataBufferTagType tagType, BinaryReader reader) {
            // new PrimitiveValue made for each switch to preserve the PrimitiveValue type tagging mechanism for numbers
            return tagType switch {
                HbcDataBufferTagType.ByteString => new PrimitiveValue(source.StringTable[reader.ReadByte()]),
                HbcDataBufferTagType.ShortString => new PrimitiveValue(source.StringTable[reader.ReadUInt16()]),
                HbcDataBufferTagType.LongString => new PrimitiveValue(source.StringTable[reader.ReadUInt32()]),
                HbcDataBufferTagType.Number => new PrimitiveValue(reader.ReadDouble()),
                HbcDataBufferTagType.Integer => new PrimitiveValue(reader.ReadInt32()),
                HbcDataBufferTagType.Null => new PrimitiveValue(null),
                HbcDataBufferTagType.True => new PrimitiveValue(true),
                HbcDataBufferTagType.False => new PrimitiveValue(false),
                _ => throw new InvalidOperationException()
            };
        }

        /// <summary>
        /// Reads the tag type (and length) for an entry in the data buffer. All subsequent items will have that type.
        /// </summary>
        private HbcDataBufferPrefix ReadTagType(BinaryReader reader) {
            const byte TAG_MASK = 0x70;

            // if the length of the data is longer than 0x0F, an additional length byte is written
            byte keyTag = reader.ReadByte();
            if ((keyTag & 0x80) == 0x80) {
                return new HbcDataBufferPrefix {
                    TagType = (HbcDataBufferTagType)(keyTag & TAG_MASK),
                    Length = (uint)(keyTag & 0x0F) << 8 | reader.ReadByte()
                };
            }
            return new HbcDataBufferPrefix {
                TagType = (HbcDataBufferTagType)(keyTag & TAG_MASK),
                Length = (uint)(keyTag & 0x0F)
            };
        }
    }
}
