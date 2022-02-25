using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits instructions that invoke functions, methods, and constructors.
    /// </summary>
    [VisitorCollection]
    public class InvokeOperations {
        /// <summary>
        /// Invokes the specified function with a given series of arguments and stores the result.
        /// </summary>
        private static void CallWithArgs(DecompilerContext context, params byte[] args) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte functionRegister = context.Instruction.Operands[1].GetValue<byte>();

            List<ISyntax> arguments = args.Select(arg => context.State.Registers[arg]).ToList();
            if (context.Decompiler.Options.OmitThisFromFunctionInvocation) {
                arguments.RemoveAt(0);
            }

            context.State.Registers[resultRegister] = new CallExpression {
                Callee = context.State.Registers[functionRegister],
                Arguments = arguments
            };
        }

        /// <summary>
        /// Either invokes a function or constructor given the specified amount of arguments,
        /// and stores the returned value.
        /// </summary>
        private static void CallOrConstruct(DecompilerContext context, bool construct) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte constructorRegister = context.Instruction.Operands[1].GetValue<byte>();
            uint argumentsCount = context.Instruction.Operands[2].GetValue<uint>();

            uint highestUsedRegister = (uint)context.State.Registers.Storage.ToList().FindLastIndex(x => x != null);
            List<ISyntax> arguments = new List<ISyntax>((int)argumentsCount);
            for (uint i = highestUsedRegister; i > highestUsedRegister - argumentsCount; i--) {
                arguments.Add(context.State.Registers[i]);
            }

            if (construct && context.Decompiler.Options.OmitPrototypeFromConstructorInvocation) {
                arguments.RemoveAt(0);
            } else if (!construct && context.Decompiler.Options.OmitThisFromFunctionInvocation) {
                arguments.RemoveAt(0);
            }

            context.State.Registers[resultRegister] = new CallExpression {
                Callee = context.State.Registers[constructorRegister],
                Arguments = arguments,
                IsCalleeConstructor = construct
            };
        }

        /// <summary>
        /// See <see cref="CallOrConstruct(DecompilerContext, bool)"/>.
        /// </summary>
        [Visitor]
        public static void Call(DecompilerContext context) {
            CallOrConstruct(context, false);
        }

        /// <summary>
        /// See <see cref="CallOrConstruct(DecompilerContext, bool)"/>.
        /// </summary>
        [Visitor]
        public static void Construct(DecompilerContext context) {
            CallOrConstruct(context, true);
        }

        /// <summary>
        /// Invokes the specified function with one parameter and stores the result.
        /// </summary>
        [Visitor]
        public static void Call1(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>());
        }

        /// <summary>
        /// Invokes the specified function with two parameters and stores the result.
        /// </summary>
        [Visitor]
        public static void Call2(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>(),
                context.Instruction.Operands[3].GetValue<byte>());
        }

        /// <summary>
        /// Invokes the specified function with three parameters and stores the result.
        /// </summary>
        [Visitor]
        public static void Call3(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>(),
                context.Instruction.Operands[3].GetValue<byte>(),
                context.Instruction.Operands[4].GetValue<byte>());
        }

        /// <summary>
        /// Invokes the specified function with four parameters and stores the result.
        /// </summary>
        [Visitor]
        public static void Call4(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>(),
                context.Instruction.Operands[3].GetValue<byte>(),
                context.Instruction.Operands[4].GetValue<byte>(),
                context.Instruction.Operands[5].GetValue<byte>());
        }
    }
}
