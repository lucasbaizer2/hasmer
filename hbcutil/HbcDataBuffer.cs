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

    public class HbcDataBuffer {
        private byte[] Buffer;

        public HbcDataBuffer(byte[] buffer) {
            Buffer = buffer;
        }

        public PrimitiveValue[] Read(HbcFile source, uint offset, uint length, out HbcDataBufferTagType tagType) {
            PrimitiveValue[] values = new PrimitiveValue[length];

            using MemoryStream ms = new MemoryStream(Buffer);
            using BinaryReader reader = new BinaryReader(ms);
            ms.Position = offset;

            tagType = ReadTagType(reader);
            for (int i = 0; i < length; i++) {
                values[i] = ReadValue(source, tagType, reader);
            }

            return values;
        }

        private PrimitiveValue ReadValue(HbcFile source, HbcDataBufferTagType tagType, BinaryReader reader) {
            // new PrimitiveValue made for each switch to preserve the PrimitiveValue type tagging mechanism for numbers
            return tagType switch {
                HbcDataBufferTagType.ByteString => new PrimitiveValue(source.StringTable[reader.ReadByte()]),
                HbcDataBufferTagType.ShortString => new PrimitiveValue(reader.ReadUInt16()),
                HbcDataBufferTagType.LongString => new PrimitiveValue(reader.ReadUInt32()),
                HbcDataBufferTagType.Number => new PrimitiveValue(reader.ReadDouble()),
                HbcDataBufferTagType.Integer => new PrimitiveValue(reader.ReadInt32()),
                HbcDataBufferTagType.Null => new PrimitiveValue(null),
                HbcDataBufferTagType.True => new PrimitiveValue(true),
                HbcDataBufferTagType.False => new PrimitiveValue(false),
                _ => throw new InvalidOperationException()
            };
        }

        private HbcDataBufferTagType ReadTagType(BinaryReader reader) {
            const byte TAG_MASK = 0x70;

            byte keyTag = reader.ReadByte();
            if ((keyTag & 0x80) == 0x80) {
                reader.BaseStream.Position += 1; // skip length value
            }
            return (HbcDataBufferTagType)(keyTag & TAG_MASK);
        }
    }
}
