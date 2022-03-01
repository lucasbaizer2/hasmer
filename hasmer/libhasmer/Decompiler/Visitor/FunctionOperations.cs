using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits instructions that depend on / change the flow of the function; i.e. loading parameters, returning values, etc.
    /// </summary>
    [VisitorCollection]
    public class FunctionOperations {
        /// <summary>
        /// Copies the value of one register into another.
        /// </summary>
        [Visitor]
        public static void CommonMov(DecompilerContext context) {
            uint toRegister = context.Instruction.Operands[0].GetValue<uint>();
            uint fromRegister = context.Instruction.Operands[1].GetValue<uint>();

            context.State.Registers[toRegister] = context.State.Registers[fromRegister];
        }

        /// <summary>
        /// Copies the value of one register into another.
        /// </summary>
        [Visitor]
        public static void Mov(DecompilerContext context) {
            byte toRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte fromRegister = context.Instruction.Operands[1].GetValue<byte>();

            // TODO: context.State.Registers.MarkUsage here?

            context.State.Registers[toRegister] = context.State.Registers[fromRegister];
        }

        /// <summary>
        /// Loads a function parameter at a given index (0 = this, 1 = first explicit parameter, etc) and stores the result.
        /// </summary>
        [Visitor]
        public static void LoadParam(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            byte paramIndex = context.Instruction.Operands[1].GetValue<byte>();
            string identifier = paramIndex switch {
                0 => "this",
                _ => "par" + (paramIndex - 1)
            };

            context.State.Registers[register] = new Identifier(identifier);
        }

        /// <summary>
        /// Gets the length of the "arguments" array and stores the result.
        /// </summary>
        [Visitor]
        public static void GetArgumentsLength(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new MemberExpression {
                Object = new Identifier("arguments"),
                Property = new Identifier("length")
            };
        }

        /// <summary>
        /// Gets an value from the arguments array by index and stores the result.
        /// </summary>
        [Visitor]
        public static void GetArgumentsPropByVal(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            byte index = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers.MarkUsage(index);

            context.State.Registers[register] = new MemberExpression {
                Object = new Identifier("arguments"),
                Property = context.State.Registers[index]
            };
        }

        /// <summary>
        /// Immediately returns out of the function with the specified return value.
        /// </summary>
        [Visitor]
        public static void Ret(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();

            // TODO: context.State.Registers.MarkUsage ?

            ReturnStatement ret = new ReturnStatement {
                // functions sometimes return an empty register
                // in that case, just coerce empty into undefined
                Argument = context.State.Registers[register] ?? new Identifier("undefined")
            };

            context.Block.Body.Add(ret);
            context.State.Registers[register] = null;
        }
    }
}
