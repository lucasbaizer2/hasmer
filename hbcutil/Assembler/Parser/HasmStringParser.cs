using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmStringToken : HasmLiteralToken {
        public string Value { get; set; }

        public HasmStringToken(HasmStringStreamState state) : base(state) { }
    }

    public class HasmStringParser : IHasmTokenParser {
        public bool CanParse(AssemblerState asm) {
            string op = asm.Stream.PeekCharacters(1);
            if (op != "\"") {
                return false;
            }

            HasmStringStreamState state = asm.Stream.SaveState();
            asm.Stream.AdvanceCharacters(1);

            char lastChar = '\0';
            while (asm.Stream.PeekCharacters(1) != null) {
                string character = asm.Stream.AdvanceCharacters(1);
                if (character == "\"" && lastChar != '\\') {
                    asm.Stream.LoadState(state);
                    return true;
                }
                lastChar = character[0];
            }

            asm.Stream.LoadState(state);
            return false;
        }

        public HasmToken Parse(AssemblerState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, "invalid string");
            }

            HasmStringStreamState state = asm.Stream.SaveState();
            asm.Stream.AdvanceCharacters(1); // skip first double quote

            asm.Stream.WhitespaceMode = HasmStringStreamWhitespaceMode.Keep;
            StringBuilder builder = new StringBuilder();
            char lastChar = '\0';
            while (asm.Stream.PeekCharacters(1) != null) {
                string character = asm.Stream.AdvanceCharacters(1);
                if (character == "\"" && lastChar != '\\') {
                    break;
                }
                builder.Append(character);
                lastChar = character[0];
            }
            asm.Stream.WhitespaceMode = HasmStringStreamWhitespaceMode.Remove;

            string toString = builder.ToString();
            return new HasmStringToken(state) {
                Value = toString
            };
        }
    }
}
