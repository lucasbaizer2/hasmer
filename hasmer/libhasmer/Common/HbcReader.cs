﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Hasmer {
    /// <summary>
    /// Represents a BinaryReader that can also read individual bits.
    /// </summary>
    public class HbcReader : BinaryReader {
        private byte? CurrentByte;
        private int Index;

        public HbcReader(Stream stream) : base(stream) {
        }

        public void Align() {
            while (BaseStream.Position % 4 != 0) {
                BaseStream.Position++;
            }
        }

        public byte ReadBit() {
            if (!CurrentByte.HasValue) {
                int read = ReadByte();
                if (read == -1) {
                    throw new EndOfStreamException();
                }
                CurrentByte = (byte)read;
                Index = 0;
            }

            byte value = (byte)((CurrentByte.Value >> Index) & 0x1);
            Index++;
            if (Index == 8) {
                CurrentByte = null;
            }

            return value;
        }

        public uint ReadBits(int amount) {
            if (amount > 32) {
                throw new IndexOutOfRangeException("cannot read more than 32 bits at once");
            }
            if (amount < 1) {
                throw new IndexOutOfRangeException("bits must be >= 1");
            }

            uint val = 0;
            for (int i = 0; i < amount; i++) {
                uint bit = ReadBit();
                val |= bit << i;
            }
            return val;
        }
    }
}
