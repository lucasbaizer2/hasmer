using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HbcUtil {
    public class HbcWriter : BinaryWriter {
        private byte? CurrentByte;
        private int Index;

        public HbcWriter(Stream stream) : base(stream) {
        }

        public void Align() {
            while (BaseStream.Position % 4 != 0) {
                BaseStream.Position++;
            }
        }

        public byte ReadBit() {
            throw new NotImplementedException();
        }

        public uint ReadBits(int amount) {
            throw new NotImplementedException();
        }
    }
}
