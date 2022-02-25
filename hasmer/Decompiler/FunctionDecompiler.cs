using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

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
            ["LoadConstUInt8"] = LoadConstUInt8,
            ["LoadConstInt"] = LoadConstInt,
            ["JNotEqual"] = JNotEqual,
            ["JStrictEqual"] = JStrictEqual,
            ["JStrictEqualLong"] = JStrictEqual,
            ["PutById"] = PutById,
            ["PutByIdLong"] = PutById,
            ["PutNewOwnById"] = PutNewOwnById,
            ["PutNewOwnByIdShort"] = PutNewOwnById,
            ["PutNewOwnByIdLong"] = PutNewOwnById,
            ["PutOwnByIndex"] = PutOwnByIndex,
            ["Construct"] = Construct,
            ["Call"] = Call,
            ["Call1"] = Call1,
            ["Call2"] = Call2,
            ["Call3"] = Call3,
            ["Call4"] = Call4,
            ["LoadParam"] = LoadParam,
            ["GetEnvironment"] = GetEnvironment,
            ["CreateEnvironment"] = CreateEnvironment,
            ["LoadFromEnvironment"] = LoadFromEnvironment,
            ["StoreToEnvironment"] = StoreToEnvironment,
            ["Ret"] = Ret,
            ["Mov"] = Mov,
            ["NewObject"] = NewObject,
            ["NewArray"] = NewArray,
            ["NewArrayWithBuffer"] = NewArrayWithBuffer,
            ["CreateClosure"] = CreateClosure,
            ["CreateThis"] = CreateThis,
            ["SelectObject"] = SelectObject
        };

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
            Instructions = header.Disassemble().ToList();
        }

        /// <summary>
        /// Writes all call expressions in all registers to the source tree, and then empties out the registers. 
        /// </summary>
        /// <param name="context"></param>
        private static void WriteRemainingRegisters(DecompilerContext context) {
            for (int i = 0; i < context.State.Registers.Length; i++) {
                ISyntax register = context.State.Registers[i];
                if (register is CallExpression) {
                    context.Block.Body.Add(register);
                    context.State.Registers[i] = null;
                }
            }
        }

        /// <summary>
        /// Decompiles a block contained within a conditional statement (e.g. JNotEqual, JLess, etc), given the boundary instruction offsets.
        /// The instruction offsets passed are the actual binary offsets, not the index in the instructions list.
        /// <br />
        /// The start parameter is exclusive, but the end parameter is inclusive.
        /// <br />
        /// Returns a new BlockStatement containing the decompiled instructions.
        /// </summary>
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

        /// <summary>
        /// Decompiles a conditional jump (e.g. JNotEqual),
        /// given the JavaScript operator that will be used in the <see cref="IfStatement"/> that will be used in the decompilation.
        /// <br />
        /// For example, for a JNotEqual instruction, the operator is "==",
        /// since the if statement is executed if the expression is equal,
        /// and the jump is performed if the expression is not equal.
        /// </summary>
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

        /// <summary>
        /// Returns an identifier representing the global object.
        /// </summary>
        private static void GetGlobalObject(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Identifier("global") {
                IsRedundant = context.Decompiler.Options.OmitExplicitGlobal
            };
        }

        /// <summary>
        /// Gets a field reference by its name (i.e. by identifier).
        /// </summary>
        private static void GetById(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();
            string identifier = context.Instruction.Operands[3].GetResolvedValue<string>(context.Source);

            context.State.Registers[resultRegister] = new MemberExpression {
                Object = context.State.Registers[sourceRegister],
                Property = new Identifier(identifier)
            };
        }

        /// <summary>
        /// Sets the value of a field reference by name (i.e. by identifier).
        /// </summary>
        private static void CommonPutById(DecompilerContext context, ISyntax property) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();

            ISyntax obj;
            if (context.State.Variables[resultRegister] != null) {
                obj = new Identifier(context.State.Variables[resultRegister]);
            } else {
                obj = context.State.Registers[resultRegister];
            }

            context.Block.Body.Add(new AssignmentExpression {
                Operator = "=",
                Left = new MemberExpression {
                    Object = obj,
                    Property = property
                },
                Right = context.State.Registers[sourceRegister]
            });
        }

        private static void PutById(DecompilerContext context) {
            CommonPutById(context, new Identifier(context.Source.StringTable[context.Instruction.Operands[3].GetValue<uint>()]));
        }

        private static void PutNewOwnById(DecompilerContext context) {
            CommonPutById(context, new Identifier(context.Source.StringTable[context.Instruction.Operands[2].GetValue<uint>()]));
        }

        private static void PutOwnByIndex(DecompilerContext context) {
            CommonPutById(context, new Literal(new PrimitiveValue(context.Instruction.Operands[2].GetValue<uint>())));
        }

        /// <summary>
        /// Creates a new JavaScript object (i.e. "{}").
        /// </summary>
        private static void NewObject(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Variables[resultRegister] = "obj" + resultRegister;
            context.State.Registers[resultRegister] = new Identifier(context.State.Variables[resultRegister]);

            context.Block.Body.Add(new AssignmentExpression {
                Left = new Identifier(context.State.Variables[resultRegister]),
                Right = new ObjectExpression(),
                Operator = "="
            });
        }

        /// <summary>
        /// Creates a new JavaScript array with a predefined length, i.e. "new Array(length)".
        /// </summary>
        private static void NewArray(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            ushort arrayLength = context.Instruction.Operands[1].GetValue<ushort>();
            context.State.Variables[resultRegister] = "arr" + resultRegister;
            context.State.Registers[resultRegister] = new Identifier(context.State.Variables[resultRegister]);

            context.Block.Body.Add(new AssignmentExpression {
                Left = context.State.Registers[resultRegister],
                Right = new CallExpression {
                    Callee = new MemberExpression {
                        Object = new Identifier("global") {
                            IsRedundant = context.Decompiler.Options.OmitExplicitGlobal
                        },
                        Property = new Identifier("Array")
                    },
                    Arguments = new List<ISyntax>() {
                        new Literal(new PrimitiveValue(arrayLength))
                    },
                    IsCalleeConstructor = true
                },
                Operator = "="
            });
        }

        /// <summary>
        /// Creates a new JavaScript array with predefined values from the array buffer.
        /// </summary>
        private static void NewArrayWithBuffer(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            ushort itemsCount = context.Instruction.Operands[2].GetValue<ushort>();
            uint arrayBufferIndex = context.Instruction.Operands[3].GetValue<uint>();

            ArrayExpression arr = new ArrayExpression();
            context.State.Variables[resultRegister] = "arr" + resultRegister;
            context.State.Registers[resultRegister] = new Identifier(context.State.Variables[resultRegister]);

            PrimitiveValue[] items = context.Decompiler.DataDisassembler.GetElementSeries(context.Decompiler.DataDisassembler.ArrayBuffer, arrayBufferIndex, itemsCount);
            arr.Elements = items.Select(item => new Literal(item)).Cast<ISyntax>().ToList();

            context.Block.Body.Add(new AssignmentExpression {
                Left = context.State.Registers[resultRegister],
                Right = arr,
                Operator = "="
            });
        }

        /// <summary>
        /// Creates a new closure.
        /// In the future, this should not just be an identifier to a closure, but should actually decompile the closure function.
        /// </summary>
        private static void CreateClosure(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            uint closureId = context.Instruction.Operands[2].GetValue<uint>();

            /*
            FunctionDecompiler closureDecompiler = new FunctionDecompiler(context.Decompiler, context.Source.SmallFuncHeaders[closureId].GetAssemblerHeader());
            ISyntax closureAST = closureDecompiler.CreateAST(context);

            context.State.Registers[resultRegister] = closureAST;
            */

            context.State.Registers[resultRegister] = new Identifier($"$closure${closureId}");
        }

        /// <summary>
        /// Creates a new "this" instance from a prototype.
        /// </summary>
        private static void CreateThis(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte prototypeRegister = context.Instruction.Operands[1].GetValue<byte>();

            // CreateThis is a VM construct more or less, so we can just consider the prototype definition as the "this" instance
            context.State.Registers[resultRegister] = context.State.Registers[prototypeRegister];
        }

        /// <summary>
        /// Gets the result of a constructor, either the 'this' instance or a returned object.
        /// For the purpose of the decompiler, it's irrelevant what the result is,
        /// so the constructor result register (from a previous call) is copied.
        /// </summary>
        private static void SelectObject(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte constructorResultRegister = context.Instruction.Operands[2].GetValue<byte>();

            context.State.Registers[resultRegister] = context.State.Registers[constructorResultRegister];
        }

        /// <summary>
        /// Gets a field reference by value (i.e. the value contained within an identifier).
        /// <br />
        /// In comparison to <see cref="GetById(DecompilerContext)"/>,
        /// GetByVal represents "obj[x]" whereas GetById represents "obj.x".
        /// </summary>
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

        /// <summary>
        /// Clears the specified register.
        /// </summary>
        private static void LoadConstEmpty(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = null;
        }

        /// <summary>
        /// Loads the JavaScript identifier "undefined" into the specified register.
        /// </summary>
        private static void LoadConstUndefined(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Identifier("undefined");
        }

        /// <summary>
        /// Loads the JavaScript literal null into the specified register.
        /// </summary>
        private static void LoadConstNull(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(null));
        }

        /// <summary>
        /// Loads the JavaScript literal boolean `true` into the specified register.
        /// </summary>
        private static void LoadConstTrue(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(true));
        }

        /// <summary>
        /// Loads the JavaScript literal boolean `false` into the specified register.
        /// </summary>
        private static void LoadConstFalse(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(false));
        }

        /// <summary>
        /// Loads the JavaScript literal number 0 into the specified register.
        /// </summary>
        private static void LoadConstZero(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(0));
        }

        /// <summary>
        /// Loads a given constant string value into the specified register.
        /// </summary>
        private static void LoadConstString(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            string str = context.Instruction.Operands[1].GetResolvedValue<string>(context.Source);
            context.State.Registers[register] = new Literal(new PrimitiveValue(str));
        }

        /// <summary>
        /// Loads a given constant numerical value into the specified register, coerced as type T.
        /// </summary>
        private static void LoadConstNumerical<T>(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            T value = context.Instruction.Operands[1].GetValue<T>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(value));
        }

        /// <summary>
        /// Loads a constant unsigned byte into the specified register.
        /// </summary>
        private static void LoadConstUInt8(DecompilerContext context) {
            LoadConstNumerical<byte>(context);
        }

        /// <summary>
        /// Loads a constant unsigned unsigned 4-byte integer into the specified register.
        /// </summary>
        private static void LoadConstInt(DecompilerContext context) {
            LoadConstNumerical<uint>(context);
        }

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

            {
                // previous way... is this right?
                /*
                List<ISyntax> arguments = new List<ISyntax>((int)argumentsCount);
                for (int i = 0; i < argumentsCount; i++) {
                    arguments.Add(context.State.Registers[constructorRegister + i + 1]);
                }
                arguments.Reverse();
                */
            }

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
        private static void Call(DecompilerContext context) {
            CallOrConstruct(context, false);
        }

        /// <summary>
        /// See <see cref="CallOrConstruct(DecompilerContext, bool)"/>.
        /// </summary>
        private static void Construct(DecompilerContext context) {
            CallOrConstruct(context, true);
        }

        /// <summary>
        /// Invokes the specified function with one parameter and stores the result.
        /// </summary>
        private static void Call1(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>());
        }

        /// <summary>
        /// Invokes the specified function with two parameters and stores the result.
        /// </summary>
        private static void Call2(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>(),
                context.Instruction.Operands[3].GetValue<byte>());
        }

        /// <summary>
        /// Invokes the specified function with three parameters and stores the result.
        /// </summary>
        private static void Call3(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>(),
                context.Instruction.Operands[3].GetValue<byte>(),
                context.Instruction.Operands[4].GetValue<byte>());
        }

        /// <summary>
        /// Invokes the specified function with four parameters and stores the result.
        /// </summary>
        private static void Call4(DecompilerContext context) {
            CallWithArgs(context,
                context.Instruction.Operands[2].GetValue<byte>(),
                context.Instruction.Operands[3].GetValue<byte>(),
                context.Instruction.Operands[4].GetValue<byte>(),
                context.Instruction.Operands[5].GetValue<byte>());
        }

        /// <summary>
        /// Loads a function parameter at a given index (0 = this, 1 = first explicit parameter, etc) and stores the result.
        /// </summary>
        private static void LoadParam(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            byte paramIndex = context.Instruction.Operands[1].GetValue<byte>();
            string identifier = paramIndex switch {
                0 => "this",
                _ => "par" + (paramIndex - 1)
            };

            context.State.Registers[register] = new Identifier(identifier);
        }

        /// <summary>
        /// Gets the environment at a given stack depth and stores the result. 0 = the current environment, 1 = the caller function's environment, etc.
        /// </summary>
        private static void GetEnvironment(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            byte environment = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers[register] = new Identifier($"__ENVIRONMENT_{environment}");
        }

        /// <summary>
        /// Creates a new environment for the current call stack frame and stores the result.
        /// </summary>
        private static void CreateEnvironment(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();

            context.State.Registers[register] = new Identifier($"__ENVIRONMENT_0");
        }

        /// <summary>
        /// Gets a symbol at a specified slot from the specified environment and stores it.
        /// </summary>
        private static void LoadFromEnvironment(DecompilerContext context) {
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
        /// Stores the specified value into the specified slot in a given environment
        /// (likely retrieved from <see cref="GetEnvironment(DecompilerContext)"/>/<see cref="CreateEnvironment(DecompilerContext)"/>).
        /// </summary>
        private static void StoreToEnvironment(DecompilerContext context) {
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

        /// <summary>
        /// Immediately returns out of the function with the specified return value.
        /// </summary>
        private static void Ret(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            ReturnStatement ret = new ReturnStatement {
                Argument = context.State.Registers[register]
            };

            context.Block.Body.Add(ret);
            context.State.Registers[register] = null;
        }

        /// <summary>
        /// Copies the value of one register into another.
        /// </summary>
        private static void Mov(DecompilerContext context) {
            byte toRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte fromRegister = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers[toRegister] = context.State.Registers[fromRegister];
        }

        /// <summary>
        /// Handles the instruction at the given instruction index, which is an offset in the <see cref="DecompilerContext.Instructions"/> list.
        /// Sets the <see cref="DecompilerContext.CurrentInstructionIndex"/> equal to the passed index parameter.
        /// If the instruction that is analyzed does not jump, then the CurrentInstructionIndex is incremented by one.
        /// Otherwise, if the instruction that is analyzed does jump,
        /// then the CurrentInstructionIndex is not modified (beyond when it was initially set to be the passed *insnIndex*).
        /// </summary>
        private static void ObserveInstruction(DecompilerContext context, int insnIndex) {
            context.CurrentInstructionIndex = insnIndex;
            HbcInstruction insn = context.Instructions[insnIndex];
            string opcodeName = context.Source.BytecodeFormat.Definitions[insn.Opcode].Name;

            if (InstructionHandlers.ContainsKey(opcodeName)) {
                InstructionHandler handler = InstructionHandlers[opcodeName];
                handler(context);
            } else {
                throw new Exception("No handler for instruction: " + opcodeName);
            }

            if (context.CurrentInstructionIndex == insnIndex) {
                context.CurrentInstructionIndex++;
            }
        }

        /// <summary>
        /// Decompiles this function into an AST structure.
        /// </summary>
        private ISyntax CreateAST(DecompilerContext parent) {
            BlockStatement block = new BlockStatement();
            FunctionDeclaration func = new FunctionDeclaration {
                Name = new Identifier(Source.SmallFuncHeaders[Header.FunctionId].GetFunctionName(Source)),
                Parameters = Header.ParamCount > 1
                                    ? Enumerable.Range(0, (int)Header.ParamCount - 1).Select(x => new Identifier($"par{x}")).ToList()
                                    : new List<Identifier>(),
                Body = block
            };
            DecompilerContext context = new DecompilerContext {
                Parent = parent,
                Decompiler = Decompiler,
                Block = block,
                CurrentInstructionIndex = 0,
                Instructions = Instructions,
            };
            context.State = new FunctionState(context, Header.FrameSize);

            while (context.CurrentInstructionIndex < context.Instructions.Count) {
                ObserveInstruction(context, context.CurrentInstructionIndex);
            }
            WriteRemainingRegisters(context);

            return func;
        }

        /// <summary>
        /// Converts the function into human-readable decompiled JavaScript.
        /// </summary>
        public string Decompile() {
            SourceCodeBuilder builder = new SourceCodeBuilder("    ");
            ISyntax ast = CreateAST(null);
            ast.Write(builder);
            return builder.ToString();
        }
    }
}
