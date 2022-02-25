using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visitors for instructions that deal with function environments.
    /// </summary>
    [VisitorCollection]
    public class EnvironmentOperations {
        /// <summary>
        /// Gets the environment at a given stack depth and stores the result. 0 = the current environment, 1 = the caller function's environment, etc.
        /// </summary>
        [Visitor]
        public static void GetEnvironment(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            byte environment = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers[register] = new Identifier($"__ENVIRONMENT_{environment}");
        }

        /// <summary>
        /// Creates a new environment for the current call stack frame and stores the result.
        /// </summary>
        [Visitor]
        public static void CreateEnvironment(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();

            context.State.Registers[register] = new Identifier($"__ENVIRONMENT_0");
        }

        /// <summary>
        /// Gets a symbol at a specified slot from the specified environment and stores it.
        /// </summary>
        [Visitor]
        public static void LoadFromEnvironment(DecompilerContext context) {
            byte destination = context.Instruction.Operands[0].GetValue<byte>();
            byte environment = context.Instruction.Operands[1].GetValue<byte>();
            ushort slot = context.Instruction.Operands[2].GetValue<ushort>();

            context.State.Registers[destination] = new MemberExpression {
                Object = context.State.Registers[environment],
                Property = new Literal(new PrimitiveValue(slot)),
                IsComputed = true
            };
        }

        /// <summary>
        /// Stores the specified value into the specified slot in a given environment,
        /// likely retrieved from <see cref="GetEnvironment(DecompilerContext)"/> or <see cref="CreateEnvironment(DecompilerContext)"/>.
        /// </summary>
        [Visitor]
        public static void StoreToEnvironment(DecompilerContext context) {
            byte environment = context.Instruction.Operands[0].GetValue<byte>();
            ushort slot = context.Instruction.Operands[1].GetValue<ushort>();
            byte valueRegister = context.Instruction.Operands[2].GetValue<byte>();

            context.Block.Body.Add(new AssignmentExpression {
                Left = new MemberExpression {
                    Object = context.State.Registers[environment],
                    Property = new Literal(new PrimitiveValue(slot)),
                },
                Right = context.State.Registers[valueRegister],
                Operator = "="
            });
            context.State.Registers[valueRegister] = null;
        }
    }
}
