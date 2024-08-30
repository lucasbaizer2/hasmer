using Hasmer.Assembler.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hasmer.Assembler.Visitor {
    /// <summary>
    /// Assembles Hasm functions.
    /// </summary>
    public class FunctionAssembler {
        /// <summary>
        /// The parent assembler, containing more data about the Hasm file.
        /// </summary>
        private HbcAssembler HbcAssembler;

        /// <summary>
        /// The token stream that reads Hasm tokens from the source code.
        /// </summary>
        private IEnumerable<HasmToken> Stream;

        /// <summary>
        /// The functions that are declared in the file.
        /// </summary>
        public List<HbcFunctionBuilder> Functions { get; set; }

        /// <summary>
        /// Creates a new function assembler instance.
        /// </summary>
        public FunctionAssembler(HbcAssembler hbcAssembler, IEnumerable<HasmToken> stream) {
            Functions = new List<HbcFunctionBuilder>();
            HbcAssembler = hbcAssembler;
            Stream = stream;
        }

        private bool DoesVariantFit(HbcInstructionDefinition concreteVariant, HasmInstructionToken insn) {
            // Console.WriteLine($"  DoesVariantFit(concreteVariant = {concreteVariant.Name}, insn = {insn.Instruction})");

            for (int i = 0; i < concreteVariant.OperandTypes.Count; i++) {
                HasmOperandToken operand = insn.Operands[i];
                HbcInstructionOperandType type = concreteVariant.OperandTypes[i];

                // Console.WriteLine($"    {i}: OperandType = {operand.OperandType}, type = {type}");

                switch (operand.OperandType) {
                    case HasmOperandTokenType.String:
                    case HasmOperandTokenType.Identifier:
                        uint stringId = HbcAssembler.DataAssembler.GetStringId(operand.Value.GetValue<string>(), operand.OperandStringKind);
                        // Console.WriteLine($"    {i}:   stringId = {stringId}, type = {type}");
                        if (!type.CanStoreInteger(stringId)) {
                            // Console.WriteLine($"    {i}:   CanStoreInteger FAILED");
                            return false;
                        }
                        break;
                    case HasmOperandTokenType.Register:
                    case HasmOperandTokenType.Label:
                    case HasmOperandTokenType.UInt:
                        ulong value = operand.Value.GetIntegerValue();
                        if (!type.CanStoreInteger(value)) {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        private HbcInstructionDefinition OptimizeInstruction(HbcInstructionDefinition def, HasmInstructionToken insn) {
            if (def.AbstractDefinition == null) {
                return def;
            }

            HbcAbstractInstructionDefinition abstractDef = HbcAssembler.File.BytecodeFormat.AbstractDefinitions[def.AbstractDefinition.Value];
            List<HbcInstructionDefinition> concreteVariants = abstractDef.VariantOpcodes.Select(variantId => HbcAssembler.File.BytecodeFormat.Definitions[(int)variantId]).ToList();
            List<HbcInstructionDefinition> validVariants = new List<HbcInstructionDefinition>(concreteVariants.Count);

            foreach (HbcInstructionDefinition concreteVariant in concreteVariants) {
                if (DoesVariantFit(concreteVariant, insn)) {
                    validVariants.Add(concreteVariant);
                }
            }

            if (validVariants.Count == 0) {
                throw new HasmParserException(insn, "no concrete defintions for variant exist for provided arguments");
            }

            validVariants.Sort((a, b) => a.TotalSize - b.TotalSize);

            return validVariants[0];
        }

        private void WriteInstruction(BinaryWriter writer, HasmInstructionToken insn) {
            string insnName = insn.Instruction;
            HbcInstructionDefinition def = HbcAssembler.File.BytecodeFormat.Definitions.Find(def => def.Name == insnName);
            if (def == null) {
                throw new HasmParserException(insn, $"unknown instruction: {insnName}");
            }

            if (!HbcAssembler.IsExact) {
                // add indentifier cache operands if not in exact mode
                if (insnName == "TryGetById" || insnName == "GetById" || insnName == "TryPutById" || insnName == "PutById") {
                    // insert a cache index of 0 as the third operand
                    insn.Operands.Insert(2, new HasmOperandToken(null) {
                        OperandType = HasmOperandTokenType.UInt,
                        Value = new PrimitiveValue((byte)0),
                    });
                }

                def = OptimizeInstruction(def, insn);
            }

            if (def.OperandTypes.Count != insn.Operands.Count) {
                throw new HasmParserException(insn, $"expecting {def.OperandTypes.Count} operands but got {insn.Operands.Count}");
            }

            writer.Write((byte)def.Opcode);

            for (int i = 0; i < def.OperandTypes.Count; i++) {
                HasmOperandToken operand = insn.Operands[i];
                HbcInstructionOperandType type = def.OperandTypes[i];

                if (operand.Value.TypeCode == TypeCode.String) {
                    uint stringId = HbcAssembler.DataAssembler.GetStringId(operand.Value.GetValue<string>(), operand.OperandStringKind);
                    if (type == HbcInstructionOperandType.UInt8S) {
                        if (stringId > byte.MaxValue) {
                            throw new Exception("string ID cannot fit into UInt8");
                        }
                        operand.Value = new PrimitiveValue((byte)stringId); // convert string to ID
                    } else if (type == HbcInstructionOperandType.UInt16S) {
                        if (stringId > ushort.MaxValue) {
                            throw new Exception("string ID cannot fit into UInt16");
                        }
                        operand.Value = new PrimitiveValue((ushort)stringId); // convert string to ID
                    } else if (type == HbcInstructionOperandType.UInt32S) {
                        if (stringId > uint.MaxValue) {
                            throw new Exception("string ID cannot fit into UInt32");
                        }
                        operand.Value = new PrimitiveValue(stringId); // convert string to ID
                    }
                } else if (type == HbcInstructionOperandType.Addr8) {
                    writer.Write(operand.Value.GetValue<sbyte>());
                } else if (type == HbcInstructionOperandType.Reg8 || type == HbcInstructionOperandType.UInt8) {
                    writer.Write(operand.Value.GetValue<byte>());
                } else if (type == HbcInstructionOperandType.Addr32) {
                    writer.Write(operand.Value.GetValue<int>());
                } else if (type == HbcInstructionOperandType.Reg32 || type == HbcInstructionOperandType.UInt32 || type == HbcInstructionOperandType.Imm32) {
                    writer.Write(operand.Value.GetValue<uint>());
                } else if (type == HbcInstructionOperandType.UInt16) {
                    writer.Write(operand.Value.GetValue<ushort>());
                } else if (type == HbcInstructionOperandType.Double) {
                    writer.Write(operand.Value.GetValue<double>());
                } else {
                    throw new Exception("invalid operand type");
                }
            }
        }

        /// <summary>
        /// Serializes the instructions of every function to a buffer sequentially.
        /// </summary>
        private byte[] BuildBytecode() {
            using MemoryStream ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);

            HbcFile file = HbcAssembler.File;
            foreach (HbcFunctionBuilder builder in Functions) {
                file.SmallFuncHeaders[builder.FunctionId].Offset = (uint)ms.Position;
                foreach (HasmInstructionToken insn in builder.Instructions) {
                    WriteInstruction(writer, insn);
                }
                file.SmallFuncHeaders[builder.FunctionId].BytecodeSizeInBytes = (uint)ms.Position - file.SmallFuncHeaders[builder.FunctionId].Offset;
            }

            return ms.ToArray();
        }

        private void AssembleFunction(HasmFunctionToken func, HbcFunctionBuilder builder) {
            foreach (HasmToken token in func.Body) {
                if (token is HasmFunctionModifierToken mod) {
                    switch (mod.ModifierType) {
                        case HasmFunctionModifierType.Id:
                            builder.FunctionId = mod.Value.Value;
                            break;
                        case HasmFunctionModifierType.Params:
                            builder.ParamCount = mod.Value.Value;
                            break;
                        case HasmFunctionModifierType.Registers:
                            builder.FrameSize = mod.Value.Value;
                            break;
                        case HasmFunctionModifierType.Symbols:
                            builder.EnvironmentSize = mod.Value.Value;
                            break;
                        case HasmFunctionModifierType.Strict:
                            builder.Flags |= HbcFuncHeaderFlags.StrictMode;
                            break;
                        default:
                            throw new Exception("invalid modifier");
                    }
                } else if (token is HasmInstructionToken insn) {
                    builder.Instructions.Add(insn);
                }
            }
        }

        public void Assemble() {
            HbcFile file = HbcAssembler.File;

            Console.WriteLine("Parsing functions...");
            foreach (HasmToken token in Stream) {
                if (token is HasmFunctionToken func) {
                    HbcFunctionBuilder builder = new HbcFunctionBuilder {
                        FunctionName = func.Name.Value,
                        FunctionId = uint.MaxValue,
                        EnvironmentSize = uint.MaxValue,
                        ParamCount = uint.MaxValue,
                        FrameSize = uint.MaxValue,
                        Flags = HbcFuncHeaderFlags.ProhibitNone,
                        Instructions = new List<HasmInstructionToken>()
                    };

                    AssembleFunction(func, builder);

                    if (builder.FunctionId == uint.MaxValue) {
                        throw new Exception($"function <{builder.FunctionName}> is missing a '.id' declaration");
                    }
                    if (builder.ParamCount == uint.MaxValue) {
                        throw new Exception($"function <{builder.FunctionName}> is missing a '.params' declaration");
                    }
                    if (builder.FrameSize == uint.MaxValue) {
                        throw new Exception($"function <{builder.FunctionName}> is missing a '.registers' declaration");
                    }
                    if (builder.EnvironmentSize == uint.MaxValue) {
                        throw new Exception($"function <{builder.FunctionName}> is missing a '.symbols' declaration");
                    }

                    Functions.Add(builder);
                }
            }

            file.SmallFuncHeaders = new HbcSmallFuncHeader[Functions.Count];
            foreach (HbcFunctionBuilder builder in Functions) {
                file.SmallFuncHeaders[builder.FunctionId] = new HbcSmallFuncHeader {
                    DeclarationFile = file,
                    FunctionName = HbcAssembler.DataAssembler.GetStringId(builder.FunctionName, StringKind.Identifier),
                    FunctionId = builder.FunctionId,
                    EnvironmentSize = builder.EnvironmentSize,
                    FrameSize = builder.FrameSize,
                    ParamCount = builder.ParamCount,
                    Flags = builder.Flags,
                };
            }

            Console.WriteLine("Building bytecode...");
            file.Instructions = BuildBytecode();
        }
    }
}
