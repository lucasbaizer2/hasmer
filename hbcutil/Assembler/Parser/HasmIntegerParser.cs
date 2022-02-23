using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmIntegerToken : HasmLiteralToken {
        private long Value { get; set; }

        public uint GetValueAsUInt32() {
            if (Value < uint.MinValue || Value > uint.MaxValue) {
                throw new HasmParserException(Line, Column, $"integer is not uint: {Value}");
            }
            return (uint)Value;
        }

        public int GetValueAsInt32() {
            if (Value < int.MinValue || Value > int.MaxValue) {
                throw new HasmParserException(Line, Column, $"integer is not int: {Value}");
            }
            return (int)Value;
        }

        public HasmIntegerToken(HasmStringStreamState state, long value) : base(state) {
            Value = value;
        }
    }

    public class HasmIntegerParser : IHasmTokenParser {
        public bool CanParse(AssemblerState asm) {
            HasmStringStreamState state = asm.Stream.SaveState();
            if (asm.Stream.PeekOperator() == "-") { // negative number
                asm.Stream.AdvanceOperator();
            }

            string word = asm.Stream.PeekWord();
            if (word == null) {
                asm.Stream.LoadState(state);
                return false;
            }

            asm.Stream.LoadState(state);
            return long.TryParse(word, out long _);
        }

        public HasmToken Parse(AssemblerState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, "invalid integer");
            }

            HasmStringStreamState state = asm.Stream.SaveState();

            int multiplier = 1;
            if (asm.Stream.PeekOperator() == "-") { // negative number
                asm.Stream.AdvanceOperator();
                multiplier = -1;
            }

            string word = asm.Stream.AdvanceWord();
            return new HasmIntegerToken(state, long.Parse(word) * multiplier);
        }
    }
}
