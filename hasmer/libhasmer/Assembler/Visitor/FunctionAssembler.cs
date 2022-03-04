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
        private HasmTokenStream Stream;

        /// <summary>
        /// The functions that are declared in the file.
        /// </summary>
        public List<HbcFunctionBuilder> Functions { get; set; }

        /// <summary>
        /// Creates a new function assembler instance.
        /// </summary>
        public FunctionAssembler(HbcAssembler hbcAssembler, HasmTokenStream stream) {
            HbcAssembler = hbcAssembler;
            Stream = stream;
        }

        private void OptimizeInstruction(ref HbcInstructionDefinition def, HasmInstructionToken token) {
            if (def.AbstractDefinition == null) {
                return;
            }

            HbcAbstractInstructionDefinition abstractDef = HbcAssembler.Header.Format.AbstractDefinitions[def.AbstractDefinition.Value];
            List<HbcInstructionDefinition> concreteVariants = abstractDef.Variants.Select(variantId => HbcAssembler.Header.Format.Definitions[(int)variantId]).ToList();
            foreach (HbcInstructionDefinition concreteVariant in concreteVariants) {
                throw new Exception("TODO: OptimizeInstruction");
            }
        }

        private void WriteInstruction(BinaryWriter writer, HasmInstructionToken insn) {
            string insnName = insn.Instruction;
            HbcInstructionDefinition def = HbcAssembler.Header.Format.Definitions.Find(def => def.Name == insnName);
            if (def == null) {
                throw new HasmParserException(insn.Line, insn.Column, $"unknown instruction: {insnName}");
            }

            if (!HbcAssembler.Header.IsExact) {
                OptimizeInstruction(ref def, insn);
            }

            writer.Write((byte)def.Opcode);

            for (int i = 0; i < def.OperandTypes.Count; i++) {
                HasmOperandToken operand = insn.Operands[i];
                HbcInstructionOperandType type = def.OperandTypes[i];

                if (operand.Value.TypeCode == TypeCode.String) {
                    uint stringId = HbcAssembler.DataAssembler.GetStringId(operand.Value.GetValue<string>());
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
                } else if (type == HbcInstructionOperandType.Addr8 || type == HbcInstructionOperandType.Reg8 || type == HbcInstructionOperandType.UInt8) {
                    writer.Write(operand.Value.GetValue<byte>());
                } else if (type == HbcInstructionOperandType.Addr32 || type == HbcInstructionOperandType.Reg32 || type == HbcInstructionOperandType.UInt32 || type == HbcInstructionOperandType.Imm32) {
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
        private void BuildBytecode() {
            using MemoryStream ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);

            HbcFile file = HbcAssembler.FileBuilder.File;
            foreach (HbcFunctionBuilder builder in Functions) {
                file.SmallFuncHeaders[builder.FunctionId].Offset = (uint)ms.Position;
                foreach (HasmInstructionToken insn in builder.Instructions) {
                    WriteInstruction(writer, insn);
                }
                file.SmallFuncHeaders[builder.FunctionId].BytecodeSizeInBytes = (uint)ms.Position - file.SmallFuncHeaders[builder.FunctionId].Offset;
            }
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
            HbcFile file = HbcAssembler.FileBuilder.File;

            foreach (HasmToken token in Stream.ReadTokens()) {
                if (token is HasmFunctionToken func) {
                    HbcFunctionBuilder builder = new HbcFunctionBuilder {
                        FunctionName = func.FunctionName,
                        FunctionId = uint.MaxValue,
                        EnvironmentSize = uint.MaxValue,
                        FrameSize = uint.MaxValue,
                        ParamCount = uint.MaxValue,
                        Flags = HbcFuncHeaderFlags.ProhibitNone,
                        Instructions = new List<HasmInstructionToken>()
                    };

                    AssembleFunction(func, builder);

                    file.SmallFuncHeaders[builder.FunctionId] = new HbcSmallFuncHeader {
                        DeclarationFile = file,
                        FunctionName = HbcAssembler.DataAssembler.GetStringId(builder.FunctionName),
                        FunctionId = builder.FunctionId,
                        EnvironmentSize = builder.EnvironmentSize,
                        FrameSize = builder.FrameSize,
                        ParamCount = builder.ParamCount,
                        Flags = builder.Flags,
                    };
                }
            }

            BuildBytecode();
        }
    }
}
