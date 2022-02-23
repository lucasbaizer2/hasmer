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

        public DecompilerContext DeepCopy() {
            ISyntax[] registers = new ISyntax[State.Registers.Length];
            Array.Copy(State.Registers, registers, registers.Length);
            string[] variables = new string[State.Variables.Length];
            Array.Copy(State.Variables, variables, variables.Length);

            return new DecompilerContext {
                Source = Source,
                Instructions = new List<HbcInstruction>(Instructions),
                State = new FunctionState((uint)State.Registers.Length) {
                    Registers = registers,
                    Variables = variables
                },
                Block = Block,
                CurrentInstructionIndex = CurrentInstructionIndex
            };
        }
    }
}
