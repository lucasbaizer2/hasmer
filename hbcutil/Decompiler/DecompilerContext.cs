using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Decompiler.AST;

namespace HbcUtil.Decompiler {
    public class DecompilerContext {
        public HbcFile Source { get; set; }
        public List<HbcInstruction> Instructions { get; set; }
        public FunctionState State { get; set; }
        public int CurrentInstructionIndex { get; set; }
        public HbcInstruction Instruction => Instructions[CurrentInstructionIndex];
        public BlockStatement Block { get; set; }
    }
}
