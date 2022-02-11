using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler {
    public class HbcDisassembler {
        public Dictionary<uint, string> DataDeclarations { get; private set; }
        public HbcFile Source { get; }

        public HbcDisassembler(HbcFile source) {
            Source = source;
            DataDeclarations = new Dictionary<uint, string>();
        }

        public string Disassemble() {
            StringBuilder functions = new StringBuilder();
            foreach (HbcSmallFuncHeader func in Source.SmallFuncHeaders) {
                FunctionDisassembler decompiler = new FunctionDisassembler(this, func.GetAssemblerHeader());
                functions.Append(decompiler.Disassemble());
                functions.AppendLine();
                functions.AppendLine();
            }

            StringBuilder output = new StringBuilder();
            List<uint> dataKeys = DataDeclarations.Keys.ToList();
            dataKeys.Sort();
            foreach (uint dataKey in dataKeys) {
                output.AppendLine(DataDeclarations[dataKey]);
                output.AppendLine();
            }

            output.AppendLine();
            output.Append(functions);

            return output.ToString();
        }
    }
}
