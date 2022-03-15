using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Hasmer.Decompiler.AST;
using Hasmer.Decompiler.Visitor;
using Hasmer.Decompiler.Analysis;

namespace Hasmer.Decompiler {
    /// <summary>
    /// Represents a decompiler for an entire function.
    /// </summary>
    public class FunctionDecompiler {
        /// <summary>
        /// Represents a handler for an individual Hermes bytecode instruction.
        /// </summary>
        private delegate void InstructionHandler(DecompilerContext context);

        /// <summary>
        /// Contains mappings between each Hermes bytecode instruction name and a function which handles the instruction for decompilation.
        /// </summary>
        private static readonly Dictionary<string, InstructionHandler> InstructionHandlers = new Dictionary<string, InstructionHandler>();

        /// <summary>
        /// The static initializer finds all <see cref="VisitorAttribute"/> annotated methods for handling instructions.
        /// </summary>
        static FunctionDecompiler() {
            Assembly visitorAssembly = typeof(VisitorCollectionAttribute).GetTypeInfo().Assembly;
            IEnumerable<Type> collections = visitorAssembly.GetTypes().Where(type => type.GetCustomAttributes<VisitorCollectionAttribute>(true).Count() > 0);
            foreach (Type collection in collections) {
                foreach (MethodInfo method in collection.GetMethods()) {
                    if (method.GetCustomAttributes<VisitorAttribute>(true).Count() > 0) {
                        if (!method.IsStatic ||
                            method.ReturnType != typeof(void) ||
                            method.GetParameters().Length != 1 ||
                            method.GetParameters()[0].ParameterType != typeof(DecompilerContext)) {
                            throw new Exception("invalid visitor: must be of signature 'public static void (DecomilerContext)'");
                        }
                        InstructionHandlers[method.Name] = context => method.Invoke(null, new object[] { context });
                    }
                }
            }
        }

        /// <summary>
        /// The parent decompiler.
        /// </summary>
        private HbcDecompiler Decompiler;

        /// <summary>
        /// The Hermes bytecode file that this function is defined in.
        /// </summary>
        private HbcFile Source => Decompiler.Source;

        /// <summary>
        /// The header of the function being decompiled.
        /// </summary>
        private HbcFuncHeader Header;

        /// <summary>
        /// Contains all bytecode instructions in the function being decompiled.
        /// </summary>
        private List<HbcInstruction> Instructions;

        /// <summary>
        /// Creates a new FunctionDecompiler given the parent HbcDecompiler and the parsed header of the function in the binary.
        /// </summary>
        public FunctionDecompiler(HbcDecompiler decompiler, HbcFuncHeader header) {
            Decompiler = decompiler;
            Header = header;
            Instructions = header.Disassemble();
        }

        /// <summary>
        /// Writes all call expressions in all registers to the source tree, and then empties out the registers. 
        /// </summary>
        /// <param name="context"></param>
        public static void WriteRemainingRegisters(DecompilerContext context) {
            for (int i = 0; i < context.State.CallExpressions.Length; i++) {
                int exprIndex = context.State.CallExpressions[i];
                if (exprIndex != -1) {
                    context.Block.Body[exprIndex] = context.State.Registers[i];
                    context.State.Registers[i] = null;
                    context.State.CallExpressions[i] = -1;
                }
            }
        }

        /// <summary>
        /// Handles the instruction at the given instruction index, which is an offset in the <see cref="DecompilerContext.Instructions"/> list.
        /// Sets the <see cref="DecompilerContext.CurrentInstructionIndex"/> equal to the passed index parameter.
        /// If the instruction that is analyzed does not jump, then the CurrentInstructionIndex is incremented by one.
        /// Otherwise, if the instruction that is analyzed does jump,
        /// then the CurrentInstructionIndex is not modified (beyond when it was initially set to be the passed *insnIndex*).
        /// </summary>
        public static void ObserveInstruction(DecompilerContext context, int insnIndex) {
            context.CurrentInstructionIndex = insnIndex;
            HbcInstruction insn = context.Instructions[insnIndex];
            string opcodeName = context.Source.BytecodeFormat.Definitions[insn.Opcode].Name;

            if (InstructionHandlers.ContainsKey(opcodeName)) {
                Console.WriteLine("Observing instruction: " + insn.ToDisassembly(context.Source));
                InstructionHandler handler = InstructionHandlers[opcodeName];
                handler(context);
            } else {
                throw new Exception($"No handler for instruction: {opcodeName}");
            }

            ControlFlowBlock beforeGraph = context.ControlFlowGraph.GetBlockContainingOffset(insn.Offset);


            if (context.CurrentInstructionIndex == insnIndex) {
                context.CurrentInstructionIndex++;

                // TODO
                /*
                ControlFlowBlock afterGraph = context.ControlFlowGraph.GetBlockAtOffset(context.Instructions[context.CurrentInstructionIndex].Offset);
                // true if a new control flow block started at the next instruction
                if (afterGraph != null) {
                    ControlFlowBlockType beforeType = context.ControlFlowGraph.GetBlockType(beforeGraph);
                    ControlFlowBlockType afterType = context.ControlFlowGraph.GetBlockType(afterGraph);

\                    if (beforeGraph.Consequent == afterGraph.BaseOffset && beforeType != ControlFlowBlockType.General && afterType == ControlFlowBlockType.General) {

                    }
                }
                */
            }
        }

        /// <summary>
        /// Decompiles this function into an AST structure.
        /// </summary>
        public SyntaxNode CreateAST(DecompilerContext parent) {
            BlockStatement block = new BlockStatement();
            DecompilerContext context = new DecompilerContext {
                Parent = parent,
                Function = Header,
                Decompiler = Decompiler,
                Block = block,
                CurrentInstructionIndex = 0,
                Instructions = Instructions,
                ControlFlowGraph = new ControlFlowGraph(Source, Instructions)
            };

            /*
            if (Header.FunctionId != 0) {
                ControlFlowGraph graph = new ControlFlowGraph(Source, Instructions);

                Console.WriteLine(graph);
            }
            */

            FunctionDeclaration func = new FunctionDeclaration {
                Name = new Identifier(Source.SmallFuncHeaders[Header.FunctionId].GetFunctionName(Source)),
                Parameters = Header.ParamCount > 1
                                    ? Enumerable.Range(0, (int)Header.ParamCount - 1).Select(x => new Identifier($"par{x}")).ToList()
                                    : new List<Identifier>(),
                Body = block,
                HbcHeader = Header
            };

            context.State = new FunctionState(context, Header.FrameSize);

            while (context.CurrentInstructionIndex < context.Instructions.Count) {
                ObserveInstruction(context, context.CurrentInstructionIndex);
            }
            WriteRemainingRegisters(context);

            StaticAnalyzer.OptimizeFunction(func);

            return func;
        }
    }
}
