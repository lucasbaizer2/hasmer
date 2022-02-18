using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HbcUtil {
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

    public class HbcDataBufferPrefix {
        public uint Length { get; set; }
        public HbcDataBufferTagType TagType { get; set; }
    }

    public class HbcDataBufferItems {
        public HbcDataBufferPrefix Prefix { get; set; }
        public PrimitiveValue[] Items { get; set; }
        public uint Offset { get; set; }
    }

    public class HbcDataBuffer {
        private byte[] Buffer;

        public HbcDataBuffer(byte[] buffer) {
            Buffer = buffer;
        }

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

        private HbcDataBufferPrefix ReadTagType(BinaryReader reader) {
            const byte TAG_MASK = 0x70;

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
