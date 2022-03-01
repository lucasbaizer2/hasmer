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
            BinaryExpression expr = new BinaryExpression {
                Left = context.State.Registers[context.Instruction.Operands[1].GetValue<byte>()],
                Right = context.State.Registers[context.Instruction.Operands[2].GetValue<byte>()],
                Operator = op
            };
            context.State.Registers[context.Instruction.Operands[0].GetValue<byte>()] = expr;
        }

        [Visitor]
        public static void StrictEq(DecompilerContext context) => DecompileBooleanOperation(context, "===");
    }
}
