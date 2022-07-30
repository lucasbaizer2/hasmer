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
        private static void AnalyzeCallUsage(DecompilerContext context, byte resultRegister, CallExpression expr) {
            // if the result of a call is used more than once, store it into a variable
            // this prevents incorrectly invoking a function multiple times in the decompilation

            // get all instructions in the function, starting after the call
            List<HbcInstruction> allInstructions = context.Function.Disassemble().Where(insn => insn.Offset > context.Instruction.Offset).ToList();

            DecompilerContext clone = context.DeepCopy();
            clone.Block = new BlockStatement();
            clone.Instructions = allInstructions;
            clone.CurrentInstructionIndex = 0;

            clone.State.Registers.RegisterUsages[resultRegister] = 0;
            clone.State.Registers[resultRegister] = expr; // put the call expression into its result register 

            bool hasBeenReferenced = false;
            bool requiresStorage = false; // whether or not we need to 
            for (; clone.CurrentInstructionIndex < clone.Instructions.Count; clone.CurrentInstructionIndex++) { // analyze each instruction individually -- don't follow through jumps
                FunctionDecompiler.ObserveInstruction(clone, clone.CurrentInstructionIndex);
                int currentUsages = clone.State.Registers.RegisterUsages[resultRegister];
                if (currentUsages > 0) {
                    if (!hasBeenReferenced) {
                        hasBeenReferenced = true;
                    }
                    if (currentUsages >= 2) { // if more than one reference was made, we need to store the call into a variable
                        requiresStorage = true;
                        break;
                    }
                } else if (hasBeenReferenced) { // if there was a reference made, but it was cleared with a different value, stop
                    break;                                                                                                                                                                                                      
                }
            }

            if (requiresStorage) {
                context.State.Registers[resultRegister] = new Identifier($"var{resultRegister}");
                context.State.Variables[resultRegister] = $"var{resultRegister}";

                context.Block.Body.Add(new AssignmentExpression {
                    Left = new Identifier($"var{resultRegister}"),
                    Right = expr,
                    Operator = "="
                });
            } else {
                context.State.Registers[resultRegister] = expr;
                context.State.CallExpressions[resultRegister] = context.Block.Body.Count; // mark the call as unused and needing to be written at some point

                context.Block.Body.Add(new EmptyExpression()); // add an empty placeholder token
            }
        }

        /// <summary>
        /// Invokes the specified function with a given series of arguments and stores the result.
        /// </summary>
        private static void CallWithArgs(DecompilerContext context, params byte[] args) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte functionRegister = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers.MarkUsages(args.Select(x => (uint)x).ToArray());

            /*
            List<SyntaxNode> arguments = args.Select(arg => context.State.Registers[arg]).ToList();
            */
            List<SyntaxNode> arguments = args.Select(arg => new Identifier($"r{arg}")).Cast<SyntaxNode>().ToList();
            if (context.Decompiler.Options.OmitThisFromFunctionInvocation) {
                arguments.RemoveAt(0);
            }
            CallExpression expr = new CallExpression {
                Callee = new Identifier($"r{functionRegister}"),
                Arguments = arguments
            };
            // AnalyzeCallUsage(context, resultRegister, expr);
            context.Block.WriteResult(resultRegister, expr);
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
            List<SyntaxNode> arguments = new List<SyntaxNode>((int)argumentsCount);
            for (uint i = highestUsedRegister; i > highestUsedRegister - argumentsCount; i--) {
                context.State.Registers.MarkUsage(i);
                arguments.Add(context.State.Registers[i]);
            }

            if (construct && context.Decompiler.Options.OmitPrototypeFromConstructorInvocation) {
                arguments.RemoveAt(0);
            } else if (!construct && context.Decompiler.Options.OmitThisFromFunctionInvocation) {
                arguments.RemoveAt(0);
            }

            CallExpression expr = new CallExpression {
                Callee = new Identifier($"r{constructorRegister}"),
                Arguments = arguments,
                IsCalleeConstructor = construct
            };
            context.Block.WriteResult(resultRegister, expr);
            // AnalyzeCallUsage(context, resultRegister, expr);
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
