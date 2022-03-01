using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Represents an internal identifier referring to an environment context.
    /// </summary>
    public class EnvironmentIdentifier : SyntaxNode {
        /// <summary>
        /// The decompiler context of the environment.
        /// </summary>
        public DecompilerContext EnvironmentContext { get; set; }

        /// <summary>
        /// The name of the function which defined the environment.
        /// </summary>
        public string EnvironmentName => EnvironmentContext.Source.StringTable[EnvironmentContext.Function.FunctionName];

        public override void Write(SourceCodeBuilder builder) {
            throw new Exception();
        }
    }

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

            DecompilerContext envContext = context.GetDeepParent(environment + 1);
            context.State.Registers[register] = new EnvironmentIdentifier {
                EnvironmentContext = envContext
            };
        }

        /// <summary>
        /// Creates a new environment for the current call stack frame and stores the result.
        /// </summary>
        [Visitor]
        public static void CreateEnvironment(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();

            context.State.Registers[register] = new EnvironmentIdentifier {
                EnvironmentContext = context
            };
        }

        /// <summary>
        /// Gets a symbol at a specified slot from the specified environment and stores it.
        /// </summary>
        [Visitor]
        public static void LoadFromEnvironment(DecompilerContext context) {
            byte destination = context.Instruction.Operands[0].GetValue<byte>();
            byte environment = context.Instruction.Operands[1].GetValue<byte>();
            ushort slot = context.Instruction.Operands[2].GetValue<ushort>();

            EnvironmentIdentifier env = (EnvironmentIdentifier)context.State.Registers[environment];
            context.State.Registers[destination] = new Identifier($"{env.EnvironmentName}${slot}");
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

            context.State.Registers.MarkUsage(valueRegister);

            EnvironmentIdentifier env = (EnvironmentIdentifier)context.State.Registers[environment];
            context.Block.Body.Add(new AssignmentExpression {
                Left = new Identifier($"{env.EnvironmentName}${slot}"),
                Right = context.State.Registers[valueRegister],
                Operator = "="
            });
        }

        /// <summary>
        /// Stores the value of NewTarget into the given register.
        /// </summary>
        [Visitor]
        public static void GetNewTarget(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();

            context.State.Registers[register] = new Identifier("NewTarget");
        }

        [Visitor]
        public static void StoreToEnvironmentL(DecompilerContext context) => StoreToEnvironment(context);

        [Visitor]
        public static void StoreNPToEnvironment(DecompilerContext context) => StoreToEnvironment(context);

        [Visitor]
        public static void StoreNPToEnvironmentL(DecompilerContext context) => StoreToEnvironment(context);
    }
}
