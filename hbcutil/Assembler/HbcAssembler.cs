using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HbcUtil.Assembler.Parser;
using HbcUtil.Assembler.Visitor;

namespace HbcUtil.Assembler {
    public class HbcAssembler {
        private string Source;

        public HbcAssembler(string source) {
            Source = source;
        }

        public byte[] Assemble() {
            HasmTokenStream stream = new HasmTokenStream(Source);
            HbcBuilder writer = new HbcBuilder(stream);
            return writer.Write();
        }
    }
}
