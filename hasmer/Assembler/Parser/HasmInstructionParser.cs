using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a Hasm instruction's operand.
    /// </summary>
    public class HasmOperandToken : HasmToken {
        /// <summary>
        /// The type of the operand.
        /// </summary>
        public HbcInstructionOperandType OperandType { get; set; }
        /// <summary>
        /// The value represented by the operand.
        /// </summary>
        public PrimitiveValue Value { get; set; }

        public HasmOperandToken(HasmStringStreamState state) : base(state) { }
    }

    /// <summary>
    /// Parses a Hasm instruction's operand.
    /// </summary>
    public class HasmOperandParser : IHasmTokenParser {
        private HbcInstructionDefinition Instruction;
        private int OperandIndex;
        private HbcInstructionOperandType Type => Instruction.OperandTypes[OperandIndex];

        public HasmOperandParser(HbcInstructionDefinition insn, int operand) {
            Instruction = insn;
            OperandIndex = operand;
        }

        public bool CanParse(HasmReaderState asm) {
            throw new NotImplementedException();
        }

        public HasmToken Parse(HasmReaderState asm) {
            HasmStringStreamState state = asm.Stream.SaveState();
            if (Type == HbcInstructionOperandType.Reg8 || Type == HbcInstructionOperandType.Reg32) {
                string reg = asm.Stream.PeekWord();
                if (reg == null || !reg.StartsWith("r") || reg.Length < 2) {
                    throw new HasmParserException(asm.Stream, "expecting register");
                }
                if (!uint.TryParse(reg.Substring(1), out uint regIndex)) {
                    throw new HasmParserException(asm.Stream, "invalid register format");
                }
                asm.Stream.AdvanceWord();

                return new HasmOperandToken(state) {
                    OperandType = Type,
                    Value = new PrimitiveValue(regIndex)
                };
            } else if (Type == HbcInstructionOperandType.Addr8 || Type == HbcInstructionOperandType.Addr32) {
                HasmLabelToken labelToken = (HasmLabelToken)IHasmTokenParser.LabelParser.Parse(asm);
                return new HasmOperandToken(state) {
                    OperandType = Type,
                    Value = new PrimitiveValue(labelToken.LabelIndex.GetValueAsUInt32())
                };
            } else if (Type == HbcInstructionOperandType.UInt8 || Type == HbcInstructionOperandType.UInt16 || Type == HbcInstructionOperandType.UInt32) {
                if ((Instruction.Name == "NewArrayWithBuffer" || Instruction.Name == "NewArrayWithBufferLong") && OperandIndex == 3) {
                    HasmLabelToken labelToken = (HasmLabelToken)IHasmTokenParser.LabelParser.Parse(asm);
                    return new HasmOperandToken(state) {
                        OperandType = Type,
                        Value = new PrimitiveValue(labelToken.LabelIndex.GetValueAsUInt32())
                    };
                }

                HasmIntegerToken intToken = (HasmIntegerToken)IHasmTokenParser.IntegerParser.Parse(asm);
                return new HasmOperandToken(state) {
                    OperandType = Type,
                    Value = new PrimitiveValue(intToken.GetValueAsUInt32())
                };
            } else if (Type == HbcInstructionOperandType.Imm32) {
                HasmIntegerToken intToken = (HasmIntegerToken)IHasmTokenParser.IntegerParser.Parse(asm);
                return new HasmOperandToken(state) {
                    OperandType = Type,
                    Value = new PrimitiveValue(intToken.GetValueAsUInt32())
                };
            } else if (Type == HbcInstructionOperandType.UInt8S || Type == HbcInstructionOperandType.UInt16S || Type == HbcInstructionOperandType.UInt32S) {
                HasmStringToken stringToken = (HasmStringToken)IHasmTokenParser.StringParser.Parse(asm);
                return new HasmOperandToken(state) {
                    OperandType = Type,
                    Value = new PrimitiveValue(stringToken.Value)
                };
            } else if (Type == HbcInstructionOperandType.Double) {
                HasmNumberToken doubleToken = (HasmNumberToken)IHasmTokenParser.NumberParser.Parse(asm);
                return new HasmOperandToken(state) {
                    OperandType = Type,
                    Value = new PrimitiveValue(doubleToken.Value)
                };
            }

            throw new NotImplementedException(Type.ToString());
        }
    }

    /// <summary>
    /// Represents a Hasm instruction.
    /// </summary>
    public class HasmInstructionToken : HasmToken {
        /// <summary>
        /// The name of the instruction.
        /// </summary>
        public string Instruction { get; set; }
        /// <summary>
        /// The operands passed to the instruction.
        /// </summary>
        public List<HasmOperandToken> Operands { get; set; }

        public HasmInstructionToken(HasmStringStreamState state) : base(state) { }
    }

    /// <summary>
    /// Parses a Hasm instruction.
    /// </summary>
    public class HasmInstructionParser : IHasmTokenParser {
        public bool CanParse(HasmReaderState asm) {
            // throw new NotImplementedException();

            string instruction = asm.Stream.PeekWord();
            return instruction != null;
        }

        public HasmToken Parse(HasmReaderState asm) {
            HasmStringStreamState state = asm.Stream.SaveState();
            string instruction = asm.Stream.AdvanceWord();

            HbcInstructionDefinition def = asm.BytecodeFormat.Definitions.Find(def => def.Name == instruction);
            if (def == null) {
                throw new HasmParserException(asm.Stream, $"unknown instruction: '{instruction}'");
            }

            HasmInstructionToken token = new HasmInstructionToken(state) {
                Instruction = instruction,
                Operands = new List<HasmOperandToken>()
            };
            for (int i = 0; i < def.OperandTypes.Count; i++) {
                if (token.Operands.Count > 0) {
                    if (asm.Stream.AdvanceOperator() != ",") {
                        throw new HasmParserException(asm.Stream, "expecting ','");
                    }
                }
                HasmOperandParser parser = new HasmOperandParser(def, i);
                token.Operands.Add((HasmOperandToken)parser.Parse(asm));
            }

            return token;
        }
    }
}
