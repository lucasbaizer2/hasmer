using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class AssemblerState {
        public HbcBytecodeFormat BytecodeFormat { get; set; }
        public HasmStringStream Stream { get; set; }
        public HasmFunctionToken CurrentFunction { get; set; }
    }
}
