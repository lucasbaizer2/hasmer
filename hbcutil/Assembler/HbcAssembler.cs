using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Assembler.Parser;

namespace HbcUtil.Assembler {
    public class HbcAssembler {
        public HbcAssembler(string source) {
            HasmTokenStream stream = new HasmTokenStream(source);
            foreach (HasmToken token in stream.ReadTokens()) {
                SourceCodeBuilder builder = new SourceCodeBuilder("    ");
                token.Write(builder);
                Console.WriteLine(builder.ToString());
            }
        }

        public byte[] Assemble() {
            return new byte[0];
        }
    }
}
