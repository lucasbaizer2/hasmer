using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits instructions that perform boolean operations.
    /// </summary>
    [VisitorCollection]
    public class BooleanOperations {
        private static void DecompileBooleanOperation(DecompilerContext context, string op) {
            byte result = context.Instruction.Operands[0].GetValue<byte>();
            byte left = context.Instruction.Operands[1].GetValue<byte>();
            byte right = context.Instruction.Operands[2].GetValue<byte>();

            context.State.Registers.MarkUsages(left, right);

            BinaryExpression expr = new BinaryExpression {
                Left = new Identifier($"r{left}"),
                Right = new Identifier($"r{right}"),
                Operator = op
            };
            // context.State.Registers[result] = expr;
            context.Block.WriteResult(result, expr);
        }

        [Visitor]
        public static void StrictEq(DecompilerContext context) => DecompileBooleanOperation(context, "===");

        [Visitor]
        public static void Less(DecompilerContext context) => DecompileBooleanOperation(context, "<");

        [Visitor]
        public static void Greater(DecompilerContext context) => DecompileBooleanOperation(context, ">");

        [Visitor]
        public static void InstanceOf(DecompilerContext context) => DecompileBooleanOperation(context, "instanceof");
    }
}
