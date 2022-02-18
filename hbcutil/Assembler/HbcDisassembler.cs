using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler {
    public class HbcDisassembler {
        public HbcFile Source { get; }
        public Dictionary<uint, string> DataDeclarations { get; private set; }
        public DataDisassembler DataDisassembler { get; private set; }

        public HbcDisassembler(HbcFile source) {
            Source = source;
            DataDeclarations = new Dictionary<uint, string>();
        }

        public string Disassemble() {
            StringBuilder builder = new StringBuilder();
            builder.Append(".hasm ");
            builder.AppendLine(Source.Header.Version.ToString());
            builder.AppendLine();

            DataDisassembler = new DataDisassembler(this);
            builder.AppendLine(DataDisassembler.Disassemble());

            foreach (HbcSmallFuncHeader func in Source.SmallFuncHeaders) {
                FunctionDisassembler decompiler = new FunctionDisassembler(this, func.GetAssemblerHeader());
                builder.AppendLine(decompiler.Disassemble());
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
