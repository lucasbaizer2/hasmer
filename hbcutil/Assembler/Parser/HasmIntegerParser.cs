using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmIntegerToken : HasmLiteralToken {
        public int Value { get; set; }

        public HasmIntegerToken(HasmStringStreamState state) : base(state) { }
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
            return int.TryParse(word, out int _);
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
            return new HasmIntegerToken(state) {
                Value = int.Parse(word) * multiplier
            };
        }
    }
}
