using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Decompiler.AST;

namespace HbcUtil.Decompiler {
    public class FunctionDecompiler {
        private delegate void InstructionHandler(DecompilerContext context);

        private static readonly Dictionary<string, InstructionHandler> InstructionHandlers = new Dictionary<string, InstructionHandler> {
            ["GetGlobalObject"] = GetGlobalObject,
            ["GetByVal"] = GetByVal,
            ["TryGetByVal"] = GetByVal,
            ["GetById"] = GetById,
            ["GetByIdShort"] = GetById,
            ["TryGetById"] = GetById,
            ["TryGetByIdLong"] = GetById,
            ["LoadConstEmpty"] = LoadConstEmpty,
            ["LoadConstUndefined"] = LoadConstUndefined,
            ["LoadConstNull"] = LoadConstNull,
            ["LoadConstTrue"] = LoadConstTrue,
            ["LoadConstFalse"] = LoadConstFalse,
            ["LoadConstZero"] = LoadConstZero,
            ["LoadConstString"] = LoadConstString,
            ["JNotEqual"] = JNotEqual,
            ["JStrictEqual"] = JStrictEqual,
            ["PutById"] = PutById,
            ["PutByIdLong"] = PutById,
            ["PutNewOwnById"] = PutById,
            ["PutNewOwnByIdShort"] = PutById,
            ["PutNewOwnByIdLong"] = PutById,
            ["Construct"] = Construct,
            ["Call2"] = Call2,
            ["Call3"] = Call3,
            ["LoadParam"] = LoadParam,
            ["GetEnvironment"] = GetEnvironment,
            ["LoadFromEnvironment"] = LoadFromEnvironment,
            ["Ret"] = Ret,
            ["Mov"] = Mov,
            ["NewObject"] = NewObject,
            ["CreateClosure"] = CreateClosure,
            ["CreateThis"] = CreateThis
        };

        private HbcFile Source;
        private HbcFuncHeader Header;
        private List<HbcInstruction> Instructions;
        private FunctionState State;

        public FunctionDecompiler(HbcFile source, HbcFuncHeader header) {
            Source = source;
            Header = header;
            Instructions = header.Disassemble().ToList();
            State = new FunctionState(header.FrameSize);
        }

        private static void WriteRemainingRegisters(DecompilerContext context) {
            for (int i = 0; i < context.State.Registers.Length; i++) {
                ISyntax register = context.State.Registers[i];
                if (register is CallExpression) {
                    context.Block.Body.Add(register);
                    context.State.Registers[i] = null;
                }
            }
        }

        private static BlockStatement DecompileConditionalBlock(DecompilerContext context, uint start, uint end) {
            BlockStatement block = new BlockStatement();

            context.Block = block;
            context.Instructions = context.Instructions.Where(x => x.Offset > start && x.Offset <= end).ToList();
            context.CurrentInstructionIndex = 0;

            while (context.CurrentInstructionIndex < context.Instructions.Count) {
                ObserveInstruction(context, context.CurrentInstructionIndex);
            }

            return block;
        }

        private static void ConditionalJump(DecompilerContext context, string op) {
            int jump = context.Instruction.Operands[0].GetValue<int>();
            byte leftRegister = context.Instruction.Operands[1].GetValue<byte>();
            byte rightRegister = context.Instruction.Operands[2].GetValue<byte>();

            IfStatement block = new IfStatement();
            block.Test = new BinaryExpression {
                Left = context.State.Registers[leftRegister],
                Right = context.State.Registers[rightRegister],
                Operator = op
            };

            if (jump < 0) {
                throw new NotImplementedException();
            } else if (jump > 0) {
                DecompilerContext consequentContext = context.DeepCopy();

                uint start = context.Instruction.Offset;
                uint to = (uint)((int)context.Instruction.Offset - (int)context.Instruction.Length + jump);

                int toIndex = context.Instructions.FindIndex(insn => insn.Offset == to);
                HbcInstruction endInstruction = context.Instructions[toIndex];
                string endInstructionName = context.Source.BytecodeFormat.Definitions[endInstruction.Opcode].Name;
                if (endInstructionName == "Jmp") { // unconditional jump means else statement
                    block.Consequent = DecompileConditionalBlock(consequentContext, start, to - endInstruction.Length); // remove the Jmp from the consequent block

                    DecompilerContext alternateContext = context.DeepCopy();
                    jump = endInstruction.Operands[0].GetValue<int>();
                    start = endInstruction.Offset;
                    to = (uint)((int)endInstruction.Offset - (int)endInstruction.Length + jump);
                    block.Alternate = DecompileConditionalBlock(alternateContext, start, to);
                    toIndex = context.Instructions.FindIndex(insn => insn.Offset == to);

                    WriteRemainingRegisters(alternateContext);
                } else {
                    block.Consequent = DecompileConditionalBlock(consequentContext, start, to);
                }

                WriteRemainingRegisters(consequentContext);
                context.CurrentInstructionIndex = toIndex + 1;
            }

            context.Block.Body.Add(block);
        }

        private static void JNotEqual(DecompilerContext context) {
            ConditionalJump(context, "==");
        }

        private static void JStrictEqual(DecompilerContext context) {
            ConditionalJump(context, "===");
        }

        private static void GetGlobalObject(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Identifier("global");
        }

        private static void GetById(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();
            string identifier = context.Instruction.Operands[3].GetResolvedValue<string>(context.Source);

            context.State.Registers[resultRegister] = new MemberExpression {
                Object = context.State.Registers[sourceRegister],
                Property = new Identifier(identifier)
            };
        }

        private static void PutById(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();
            uint identifierRegister = context.Instruction.Operands[2].GetValue<uint>();

            context.Block.Body.Add(new AssignmentExpression {
                Operator = "=",
                Left = new MemberExpression {
                    Object = new Identifier(context.State.Variables[resultRegister]),
                    Property = new Identifier(context.Source.StringTable[identifierRegister])
                },
                Right = context.State.Registers[sourceRegister]
            });
        }

        private static void NewObject(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Variables[resultRegister] = "var" + resultRegister;

            context.Block.Body.Add(new AssignmentExpression {
                Left = new Identifier(context.State.Variables[resultRegister]),
                Right = new ObjectExpression(),
                Operator = "="
            });
        }

        private static void CreateClosure(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            uint closureId = context.Instruction.Operands[2].GetValue<uint>();

            context.State.Registers[resultRegister] = new Identifier($"$closure${closureId}");
        }

        private static void CreateThis(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte prototypeRegister = context.Instruction.Operands[1].GetValue<byte>();

            // CreateThis is a VM construct more or less, so we can just consider the prototype definition as the "this" instance
            context.State.Registers[resultRegister] = context.State.Registers[prototypeRegister];
        }

        private static void GetByVal(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();
            uint identifierRegister = context.Instruction.Operands[2].GetValue<uint>();

            context.State.Registers[resultRegister] = new MemberExpression(false) {
                Object = context.State.Registers[sourceRegister],
                Property = context.State.Registers[identifierRegister],
                IsComputed = true
            };
        }

        private static void LoadConstEmpty(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = null;
        }

        private static void LoadConstUndefined(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Identifier("undefined");
        }

        private static void LoadConstNull(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(null));
        }

        private static void LoadConstTrue(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(true));
        }

        private static void LoadConstFalse(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(false));
        }

        private static void LoadConstZero(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(0));
        }

        private static void LoadConstString(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            string str = context.Instruction.Operands[1].GetResolvedValue<string>(context.Source);
            context.State.Registers[register] = new Literal(new PrimitiveValue(str));
        }

        private static void CallWithArgs(DecompilerContext context, params byte[] args) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte functionRegister = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers[resultRegister] = new CallExpression {
                Callee = context.State.Registers[functionRegister],
                Arguments = args.Select(arg => context.State.Registers[arg]).ToList()
            };
            context.State.Registers[functionRegister] = null;
        }

        private static void Construct(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte constructorRegister = context.Instruction.Operands[1].GetValue<byte>();
            uint argumentsCount = context.Instruction.Operands[2].GetValue<uint>();

            List<ISyntax> arguments = new List<ISyntax>((int)argumentsCount);
            for (int i = 0; i < argumentsCount; i++) {
                arguments.Add(new Literal(new PrimitiveValue(null)));
            }

            context.State.Registers[resultRegister] = new UnaryExpression {
                Operator = "new",
                Argument = new CallExpression {
                    Callee = context.State.Registers[constructorRegister],
                    Arguments = arguments
                }
            };
        }

        private static void Call2(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>(),
                context.Instruction.Operands[3].GetValue<byte>());
        }

        private static void Call3(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>(),
                context.Instruction.Operands[3].GetValue<byte>(),
                context.Instruction.Operands[4].GetValue<byte>());
        }

        private static void LoadParam(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            byte paramIndex = context.Instruction.Operands[1].GetValue<byte>();
            string identifier = paramIndex switch {
                0 => "this",
                _ => "par" + (paramIndex - 1)
            };

            context.State.Registers[register] = new Identifier(identifier);
        }

        private static void GetEnvironment(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            byte environment = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers[register] = new Identifier($"__ENVIRONMENT_{environment}");
        }

        private static void LoadFromEnvironment(DecompilerContext context) {
            byte destination = context.Instruction.Operands[0].GetValue<byte>();
            byte environment = context.Instruction.Operands[1].GetValue<byte>();
            byte slot = context.Instruction.Operands[2].GetValue<byte>();

            context.State.Registers[destination] = new MemberExpression {
                Object = context.State.Registers[environment],
                Property = new Literal(new PrimitiveValue(slot)),
                IsComputed = true
            };
        }

        private static void Ret(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            ReturnStatement ret = new ReturnStatement {
                Argument = context.State.Registers[register]
            };

            context.Block.Body.Add(ret);
            context.State.Registers[register] = null;
        }

        private static void Mov(DecompilerContext context) {
            byte toRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte fromRegister = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers[toRegister] = context.State.Registers[fromRegister];
        }

        private static void ObserveInstruction(DecompilerContext context, int insnIndex) {
            context.CurrentInstructionIndex = insnIndex;
            HbcInstruction insn = context.Instructions[insnIndex];
            string opcodeName = context.Source.BytecodeFormat.Definitions[insn.Opcode].Name;

            if (InstructionHandlers.ContainsKey(opcodeName)) {
                InstructionHandler handler = InstructionHandlers[opcodeName];
                handler(context);
            } else {
                Console.WriteLine("No handler for instruction: " + opcodeName);
            }

            if (context.CurrentInstructionIndex == insnIndex) {
                context.CurrentInstructionIndex++;
            }
        }

        /// <summary>
        /// Converts the function into human-readable decompiled JavaScript.
        /// </summary>
        public string Decompile() {
            BlockStatement block = new BlockStatement();
            FunctionDeclaration func = new FunctionDeclaration {
                Name = new Identifier(Source.StringTable[Header.FunctionName]),
                Parameters = Header.ParamCount > 1
                                    ? Enumerable.Range(0, (int)Header.ParamCount - 1).Select(x => new Identifier($"par{x}")).ToList()
                                    : new List<Identifier>(),
                Body = block
            };
            DecompilerContext context = new DecompilerContext {
                Block = block,
                Source = Source,
                CurrentInstructionIndex = 0,
                Instructions = Instructions,
                State = new FunctionState(Header.FrameSize)
            };

            while (context.CurrentInstructionIndex < context.Instructions.Count) {
                ObserveInstruction(context, context.CurrentInstructionIndex);
            }
            WriteRemainingRegisters(context);

            State.DebugPrint();

            Console.WriteLine("----------------------------");

            SourceCodeBuilder builder = new SourceCodeBuilder("    ");
            func.Write(builder);

            Console.WriteLine(builder.ToString());
            return builder.ToString();
        }
    }
}
