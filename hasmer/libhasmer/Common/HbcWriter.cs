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
        // private byte? CurrentByte;
        // private int Index;

        public HbcWriter(Stream stream) : base(stream) {
        }

        public void Align() {
            while (BaseStream.Position % 4 != 0) {
                Write((byte)0);
            }
        }

        public void WriteBit(byte bit) {
            throw new NotImplementedException();
        }

        public void WriteBits(uint value, int bitsToWrite) {
            throw new NotImplementedException();
        }
    }
}
