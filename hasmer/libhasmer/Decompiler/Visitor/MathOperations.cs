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
            byte result = context.Instruction.Operands[0].GetValue<byte>();
            byte left = context.Instruction.Operands[1].GetValue<byte>();
            byte right = context.Instruction.Operands[2].GetValue<byte>();

            context.State.Registers.MarkUsages(left, right);

            BinaryExpression expr = new BinaryExpression {
                Left = new Identifier($"r{left}"),
                Right = new Identifier($"r{right}"),
                Operator = op
            };
            context.Block.WriteResult(result, expr);
            // context.State.Registers[result] = expr;
        }

        [Visitor]
        public static void Add(DecompilerContext context) => DecompileBinaryOperation(context, "+");

        [Visitor]
        public static void AddN(DecompilerContext context) => DecompileBinaryOperation(context, "+");

        [Visitor]
        public static void Sub(DecompilerContext context) => DecompileBinaryOperation(context, "-");

        [Visitor]
        public static void SubN(DecompilerContext context) => DecompileBinaryOperation(context, "-");

        [Visitor]
        public static void Mul(DecompilerContext context) => DecompileBinaryOperation(context, "*");

        [Visitor]
        public static void MulN(DecompilerContext context) => DecompileBinaryOperation(context, "*");
    }
}
