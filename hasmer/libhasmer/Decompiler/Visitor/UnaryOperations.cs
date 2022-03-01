using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits instructions whose decompiled AST is a <see cref="UnaryExpression"/>, such as TypeOf.
    /// </summary>
    [VisitorCollection]
    public class UnaryOperations {
        /// <summary>
        /// Directly equivalent to the "typeof" operator in JavaScript; gets the type of a given register and stores it into another register.
        /// </summary>
        [Visitor]
        public static void TypeOf(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers.MarkUsage(sourceRegister);

            context.State.Registers[resultRegister] = new UnaryExpression {
                Operator = "typeof",
                Argument = context.State.Registers[sourceRegister]
            };
        }
    }
}
