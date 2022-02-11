using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HbcUtil.Decompile {
    public class HbcDecompiler {
        private HbcFile Source;

        public HbcDecompiler(HbcFile source) {
            Source = source;
        }

        public void Decompile() {
            foreach (HbcSmallFuncHeader func in Source.SmallFuncHeaders) {
                if (Source.StringTable[func.FunctionName] == "define" && func.FrameSize == 7) {
                    FunctionDecompiler decompiler = new FunctionDecompiler(Source, func.GetAssemblerHeader());
                    decompiler.Decompile();
                }
            }
        }
    }
}
