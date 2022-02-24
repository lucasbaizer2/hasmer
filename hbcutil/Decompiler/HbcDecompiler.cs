using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HbcUtil.Decompiler {
    public class HbcDecompiler {
        private HbcFile Source;

        public HbcDecompiler(HbcFile source) {
            Source = source;
        }

        /// <summary>
        /// Converts the bytecode file into human-readable diassembled JavaScript.
        /// </summary>
        public string Decompile() {
            StringBuilder builder = new StringBuilder();

            foreach (HbcSmallFuncHeader func in Source.SmallFuncHeaders) {
                if (func.FunctionId == 173 || func.FunctionId == 174) {
                    FunctionDecompiler decompiler = new FunctionDecompiler(Source, func.GetAssemblerHeader());
                    string decompiled = decompiler.Decompile();
                    builder.AppendLine(decompiled);
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}
