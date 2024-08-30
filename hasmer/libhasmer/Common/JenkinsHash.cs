using System;

namespace Hasmer {
    public class JenkinsHash {
        private uint State;

        public void Update(uint x) {
            this.State = Mix2(Mix1(this.State + x));
        }

        public uint Finalize() {
            uint s = this.State;
            this.State = 0;
            return s;
        }

        private static uint Mix1(uint h) {
            return h + (h << 10);
        }

        private static uint Mix2(uint h) {
            return h ^ (h >> 6);
        }

        public static uint Hash(ReadOnlySpan<byte> data, bool isUTF16) {
            JenkinsHash h = new JenkinsHash();
            if (isUTF16) {
                if (data.Length % 2 != 0) {
                    throw new Exception("UTF-16 data length should be a multiple of 2");
                }

                for (int i = 0; i < data.Length - 1; i += 2) {
                    uint x = BitConverter.ToUInt16(data.Slice(start: i, length: 2));
                    h.Update(x);
                }
            } else {
                foreach (byte x in data) {
                    h.Update(x);
                }
            }
            return h.Finalize();
        }
    }
}
