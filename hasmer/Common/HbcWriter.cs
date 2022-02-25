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
                BaseStream.Position++;
            }
        }

        public void WriteBit(byte bit) {
            throw new NotImplementedException();
        }

        public void WriteBits(uint bits) {
            throw new NotImplementedException();
        }
    }
}
