using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Decompile.AST;

namespace HbcUtil.Decompile {
    public class FunctionDecompiler {
        private delegate void InstructionHandler(DecompilerContext context);

        private static readonly Dictionary<string, InstructionHandler> InstructionHandlers = new Dictionary<string, InstructionHandler> {
            ["GetGlobalObject"] = GetGlobalObject,
            ["TryGetById"] = GetById,
            ["GetByIdShort"] = GetById,
            ["GetByVal"] = GetByVal,
            ["LoadConstEmpty"] = LoadConstEmpty,
            ["LoadConstUndefined"] = LoadConstUndefined,
            ["LoadConstNull"] = LoadConstNull,
            ["LoadConstTrue"] = LoadConstTrue,
            ["LoadConstFalse"] = LoadConstFalse,
            ["LoadConstZero"] = LoadConstZero,
            ["JNotEqual"] = JNotEqual
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
            BlockStatement block = new BlockStatement();
            sbyte jump = context.Instruction.Operands[0].GetValue<sbyte>();
            if (jump < 0) {
                // not yet implemented
            } else if (jump > 0) {

            }
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

        private static void GetByVal(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();
            byte identifierRegister = context.Instruction.Operands[2].GetValue<byte>();

            context.State.Registers[resultRegister] = new MemberExpression {
                Object = context.State.Registers[sourceRegister],
                Property = context.State.Registers[identifierRegister]
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

        private void ObserveInstruction(BlockStatement block, int insnIndex) {
            HbcInstruction insn = Instructions[insnIndex];
            string opcodeName = Source.BytecodeFormat.Definitions[insn.Opcode].Name;

            if (opcodeName == "Call2") {

            } else if (opcodeName == "LoadParam") {

            } else if (opcodeName == "GetEnvironment") {
                byte register = insn.Operands[0].GetValue<byte>();
                byte environment = insn.Operands[1].GetValue<byte>();

                State.Registers[register] = new Identifier($"__ENVIRONMENT_{environment}");
            } else if (opcodeName == "LoadFromEnvironment") {
                byte destination = insn.Operands[0].GetValue<byte>();
                byte environment = insn.Operands[1].GetValue<byte>();
                byte slot = insn.Operands[2].GetValue<byte>();

                State.Registers[destination] = new MemberExpression {
                    Object = State.Registers[environment],
                    Property = new Literal(new PrimitiveValue(slot)),
                    IsComputed = true
                };
            } else if (opcodeName == "Ret") {
                byte register = insn.Operands[0].GetValue<byte>();
                ReturnStatement ret = new ReturnStatement {
                    Argument = State.Registers[register]
                };

                block.Body.Add(ret);
            }

            if (insnIndex + 1 < Instructions.Count) {
                ObserveInstruction(block, insnIndex + 1);
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

            ObserveInstruction(block, 0);

            State.DebugPrint();

            Console.WriteLine("----------------------------");

            SourceCodeBuilder builder = new SourceCodeBuilder("    ");
            func.Write(builder);
            Console.WriteLine(builder.ToString());
        }
    }
}
