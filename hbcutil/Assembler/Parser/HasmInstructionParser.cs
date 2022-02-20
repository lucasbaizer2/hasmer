using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmOperandToken : HasmToken {
        public HbcInstructionOperandType OperandType { get; set; }
        public PrimitiveValue Value { get; set; }

        public HasmOperandToken(HasmStringStreamState state) : base(state) { }
    }

    public class HasmOperandParser : IHasmTokenParser {
        private HbcInstructionOperandType Type;

        public HasmOperandParser(HbcInstructionOperandType type) {
            Type = type;
        }

        public bool CanParse(AssemblerState asm) {
            throw new NotImplementedException();
        }

        public HasmToken Parse(AssemblerState asm) {
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
                    Value = new PrimitiveValue((uint)labelToken.LabelIndex.Value)
                };
            } else if (Type == HbcInstructionOperandType.UInt16S || Type == HbcInstructionOperandType.UInt32S) {
                HasmStringToken stringToken = (HasmStringToken)IHasmTokenParser.StringParser.Parse(asm);
                return new HasmOperandToken(state) {
                    OperandType = Type,
                    Value = new PrimitiveValue(stringToken.Value)
                };
            }

            throw new NotImplementedException();
        }
    }

    public class HasmInstructionToken : HasmToken {
        public string Instruction { get; set; }
        public List<HasmOperandToken> Operands { get; set; }

        public HasmInstructionToken(HasmStringStreamState state) : base(state) { }
    }

    public class HasmInstructionParser : IHasmTokenParser {
        public bool CanParse(AssemblerState asm) {
            // throw new NotImplementedException();

            string instruction = asm.Stream.PeekWord();
            return instruction != null;
        }

        public HasmToken Parse(AssemblerState asm) {
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
            foreach (HbcInstructionOperandType type in def.OperandTypes) {
                HasmOperandParser parser = new HasmOperandParser(type);
                token.Operands.Add((HasmOperandToken)parser.Parse(asm));
            }

            return token;
        }
    }
}
