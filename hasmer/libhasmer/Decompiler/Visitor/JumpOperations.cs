using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;
using Hasmer.Decompiler.Analysis;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits instructions that perform jump based on given citeria.
    /// </summary>
    [VisitorCollection]
    public class JumpOperations {
        /// <summary>
        /// Decompiles a block contained within a conditional statement (e.g. JNotEqual, JLess, etc), given the boundary instruction offsets.
        /// The instruction offsets passed are the actual binary offsets, not the index in the instructions list.
        /// <br />
        /// The start parameter is exclusive, but the end parameter is inclusive.
        /// <br />
        /// Returns a new BlockStatement containing the decompiled instructions.
        /// </summary>
        private static BlockStatement DecompileConditionalBlock(DecompilerContext context, List<HbcInstruction> instructions) {
            BlockStatement block = new BlockStatement();

            context.Block = block;
            context.Instructions = instructions;
            context.CurrentInstructionIndex = 0;

            while (context.CurrentInstructionIndex < context.Instructions.Count) {
                FunctionDecompiler.ObserveInstruction(context, context.CurrentInstructionIndex);
            }

            return block;
        }

        /// <summary>
        /// Decompiles a conditional jump (e.g. JNotEqual),
        /// given the JavaScript operator that will be used in the <see cref="IfStatement"/> that will be used in the decompilation.
        /// <br />
        /// For example, for a JNotEqual instruction, the operator is "==",
        /// since the if statement is executed if the expression is equal,
        /// and the jump is performed if the expression is not equal.
        /// </summary>
        private static void ConditionalJump(DecompilerContext context, SyntaxNode expr) {
            int jump = context.Instruction.Operands[0].GetValue<int>();

            IfStatement ifBlock = new IfStatement {
                Test = expr
            };

            if (jump < 0) {
                throw new NotImplementedException();
            } else if (jump > 0) {
                ControlFlowBlock jumpingBlock = context.ControlFlowGraph.GetBlockContainingOffset(context.Instruction.Offset);

                DecompilerContext consequentContext = context.DeepCopy();
                ControlFlowBlock consequentControlBlock = context.ControlFlowGraph.GetBlockAtOffset(jumpingBlock.Consequent.Value);
                ifBlock.Consequent = DecompileConditionalBlock(consequentContext, context.ControlFlowGraph.GetBlockInstructions(consequentControlBlock).ToList());

                DecompilerContext alternateContext = context.DeepCopy();
                ControlFlowBlock alternateControlBlock = context.ControlFlowGraph.GetBlockAtOffset(jumpingBlock.Alternate.Value);
                ifBlock.Alternate = DecompileConditionalBlock(alternateContext, context.ControlFlowGraph.GetBlockInstructions(alternateControlBlock).ToList());

                FunctionDecompiler.WriteRemainingRegisters(consequentContext);
                FunctionDecompiler.WriteRemainingRegisters(alternateContext);

                // uint largestOffset = Math.Max(consequentControlBlock.BaseOffset + consequentControlBlock.Length, alternateControlBlock.BaseOffset + alternateControlBlock.Length);
                // int largestIndex = context.Instructions.FindIndex(insn => insn.Offset == largestOffset);
                // context.CurrentInstructionIndex = largestIndex;
                context.CurrentInstructionIndex = context.Instructions.Count;
            }

            context.Block.Body.Add(ifBlock);
        }

        private static void JumpBinaryExpression(DecompilerContext context, string op) {
            byte left = context.Instruction.Operands[1].GetValue<byte>();
            byte right = context.Instruction.Operands[2].GetValue<byte>();

            context.State.Registers.MarkUsages(left, right);

            ConditionalJump(context, new BinaryExpression {
                Left = context.State.Registers[left],
                Right = context.State.Registers[right],
                Operator = op
            });
        }

        [Visitor]
        public static void Jmp(DecompilerContext _) {
            
        }

        [Visitor]
        public static void JEqual(DecompilerContext context) => JumpBinaryExpression(context, "==");

        [Visitor]
        public static void JNotEqual(DecompilerContext context) => JumpBinaryExpression(context, "!=");

        [Visitor]
        public static void JStrictEqual(DecompilerContext context) => JumpBinaryExpression(context, "===");

        [Visitor]
        public static void JStrictNotEqual(DecompilerContext context) => JumpBinaryExpression(context, "!==");

        [Visitor]
        public static void JNotGreater(DecompilerContext context) => JumpBinaryExpression(context, "<=");

        [Visitor]
        public static void JNotLess(DecompilerContext context) => JumpBinaryExpression(context, ">=");

        [Visitor]
        public static void JNotLessEqual(DecompilerContext context) => JumpBinaryExpression(context, ">");

        [Visitor]
        public static void JGreater(DecompilerContext context) => JumpBinaryExpression(context, ">");

        [Visitor]
        public static void JGreaterEqual(DecompilerContext context) => JumpBinaryExpression(context, ">=");

        [Visitor]
        public static void JLess(DecompilerContext context) => JumpBinaryExpression(context, "<");

        [Visitor]
        public static void JLessEqual(DecompilerContext context) => JumpBinaryExpression(context, "<=");

        [Visitor]
        public static void JmpTrue(DecompilerContext context) {
            byte arg = context.Instruction.Operands[1].GetValue<byte>();
            context.State.Registers.MarkUsage(arg);
            ConditionalJump(context, new UnaryExpression {
                Operator = "!",
                Argument = context.State.Registers[arg]
            });
        }

        [Visitor]
        public static void JmpFalse(DecompilerContext context) {
            uint arg = context.Instruction.Operands[1].GetValue<uint>();
            context.State.Registers.MarkUsage(arg);
            ConditionalJump(context, context.State.Registers[arg]);
        }

        [Visitor]
        public static void JmpFalseLong(DecompilerContext context) => JmpFalse(context);
    }
}
