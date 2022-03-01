using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits instructions that perform mathematical operations (e.g. addition, subtraction, etc.)
    /// </summary>
    [VisitorCollection]
    public class MathOperations {
        private static void DecompileBinaryOperation(DecompilerContext context, string op) {
            BinaryExpression expr = new BinaryExpression {
                Left = context.State.Registers[context.Instruction.Operands[1].GetValue<byte>()],
                Right = context.State.Registers[context.Instruction.Operands[2].GetValue<byte>()],
                Operator = op
            };
            context.State.Registers[context.Instruction.Operands[0].GetValue<byte>()] = expr;
        }

        [Visitor]
        public static void Add(DecompilerContext context) => DecompileBinaryOperation(context, "+");

        [Visitor]
        public static void AddN(DecompilerContext context) => DecompileBinaryOperation(context, "+");

        [Visitor]
        public static void Sub(DecompilerContext context) => DecompileBinaryOperation(context, "-");

        [Visitor]
        public static void SubN(DecompilerContext context) => DecompileBinaryOperation(context, "-");
    }
}
