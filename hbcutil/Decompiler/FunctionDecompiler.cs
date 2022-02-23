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
            ["TryGetById"] = GetById,
            ["GetByVal"] = GetByVal,
            ["GetByIdShort"] = GetById,
            ["TryGetByVal"] = GetByVal,
            ["TryGetByIdLong"] = GetById,
            ["LoadConstEmpty"] = LoadConstEmpty,
            ["LoadConstUndefined"] = LoadConstUndefined,
            ["LoadConstNull"] = LoadConstNull,
            ["LoadConstTrue"] = LoadConstTrue,
            ["LoadConstFalse"] = LoadConstFalse,
            ["LoadConstZero"] = LoadConstZero,
            ["JNotEqual"] = JNotEqual,
            ["PutById"] = PutById,
            ["PutByIdLong"] = PutById,
            ["PutNewOwnById"] = PutById,
            ["PutNewOwnByIdShort"] = PutById,
            ["PutNewOwnByIdLong"] = PutById,
            ["Call2"] = Call2,
            ["LoadParam"] = LoadParam,
            ["GetEnvironment"] = GetEnvironment,
            ["LoadFromEnvironment"] = LoadFromEnvironment,
            ["Ret"] = Ret,
            ["NewObject"] = NewObject
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

        private static void JNotEqual(DecompilerContext context) {
            sbyte jump = context.Instruction.Operands[0].GetValue<sbyte>();
            byte leftRegister = context.Instruction.Operands[1].GetValue<byte>();
            byte rightRegister = context.Instruction.Operands[2].GetValue<byte>();

            IfStatement block = new IfStatement();
            block.Consequent = new BlockStatement();
            block.Test = new BinaryExpression {
                Left = context.State.Registers[leftRegister],
                Right = context.State.Registers[rightRegister],
                Operator = "=="
            };

            if (jump < 0) {
                throw new NotImplementedException();
            } else if (jump > 0) {
                DecompilerContext contextCopy = context.DeepCopy();
                contextCopy.Block = block.Consequent;
                contextCopy.Instructions = context.Instructions.Where(x => x.Offset > context.Instruction.Offset && x.Offset < context.Instruction.Offset + jump).ToList();
                contextCopy.CurrentInstructionIndex = 0;

                int insnIndex = 0;
                while (insnIndex < contextCopy.Instructions.Count) {
                    ObserveInstruction(contextCopy, insnIndex);
                    insnIndex++;
                }

                int to = (int)context.Instruction.Offset + jump;
                int toIndex = context.Instructions.FindIndex(insn => insn.Offset == to);
                context.CurrentInstructionIndex = toIndex - 1;
            }
            context.Block.Body.Add(block);
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

        private static void Call2(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte functionRegister = context.Instruction.Operands[1].GetValue<byte>();
            byte arg0Register = context.Instruction.Operands[2].GetValue<byte>();
            byte arg1Register = context.Instruction.Operands[3].GetValue<byte>();

            context.State.Registers[resultRegister] = new CallExpression {
                Callee = context.State.Registers[functionRegister],
                Arguments = new List<ISyntax>() {
                        context.State.Registers[arg0Register],
                        context.State.Registers[arg1Register]
                    }
            };
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
        }

        private static void ObserveInstruction(DecompilerContext context, int insnIndex) {
            context.CurrentInstructionIndex = insnIndex;
            HbcInstruction insn = context.Instructions[insnIndex];
            string opcodeName = context.Source.BytecodeFormat.Definitions[insn.Opcode].Name;

            if (InstructionHandlers.ContainsKey(opcodeName)) {
                InstructionHandler handler = InstructionHandlers[opcodeName];
                handler(context);
            }

            if (context.CurrentInstructionIndex == insnIndex) {
                context.CurrentInstructionIndex++;
            }
        }

        public void Decompile() {
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

            State.DebugPrint();

            Console.WriteLine("----------------------------");

            SourceCodeBuilder builder = new SourceCodeBuilder("    ");
            func.Write(builder);
            Console.WriteLine(builder.ToString());
        }
    }
}
