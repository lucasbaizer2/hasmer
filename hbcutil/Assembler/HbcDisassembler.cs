using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler {
    public class HbcDisassembler {
        private HbcFile Source;

        public HbcDisassembler(HbcFile source) {
            Source = source;
        }

        public string Disassemble() {
            StringBuilder output = new StringBuilder();
            foreach (HbcSmallFuncHeader func in Source.SmallFuncHeaders) {
                FunctionDisassembler decompiler = new FunctionDisassembler(Source, func.GetAssemblerHeader());
                output.Append(decompiler.Disassemble());
                output.AppendLine();
                output.AppendLine();
            }
            return output.ToString();
        }
    }
}
