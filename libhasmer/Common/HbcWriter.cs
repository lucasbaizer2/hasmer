using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Hasmer {
    /// <summary>
    /// Represents a BinaryWriter which can write individual bits.
    /// </summary>
    public class HbcWriter : BinaryWriter {
        private byte CurrentByte;
        private byte Index;

        public HbcWriter(Stream stream) : base(stream) {
        }

        public void Align() {
            while (BaseStream.Position % 4 != 0) {
                Write((byte)0);
            }
        }

        public void WriteBit(byte bit) {
            if (bit == 0) {
                CurrentByte &= (byte)~(1 << Index++);
            } else {
                CurrentByte |= (byte)(1 << Index++);
            }
            if (Index == 8) {
                Write(CurrentByte);

                CurrentByte = 0;
                Index = 0;
            }
        }

        public void WriteBits(uint value, int bitsToWrite) {
            if (bitsToWrite > 32) {
                throw new IndexOutOfRangeException("cannot write more than 32 bits at once");
            }
            if (bitsToWrite < 1) {
                throw new IndexOutOfRangeException("bits must be >= 1");
            }
            for (int i = bitsToWrite - 1; i >= 0; i--) {
                byte currentBit = (byte)((value >> i) & 1);
                WriteBit(currentBit);
            }
        }
    }
}
