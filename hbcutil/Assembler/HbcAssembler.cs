using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Assembler.Parser;
using System.IO;

namespace HbcUtil.Assembler {
    public class HbcAssembler {
        public HbcAssembler(string source) {
            using FileStream fs = File.OpenWrite("debug.json");

            HasmTokenStream stream = new HasmTokenStream(source);
            foreach (HasmToken token in stream.ReadTokens()) {
                SourceCodeBuilder builder = new SourceCodeBuilder("    ");
                token.Write(builder);
                fs.Write(Encoding.UTF8.GetBytes(builder.ToString()));
                fs.Flush();
            }
        }

        public byte[] Assemble() {
            return new byte[0];
        }
    }
}
