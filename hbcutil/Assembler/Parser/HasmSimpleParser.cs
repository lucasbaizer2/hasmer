using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmSimpleToken : HasmLiteralToken {
        public string Value { get; set; }

        public HasmSimpleToken(HasmStringStreamState state) : base(state) { }
    }

    public class HasmSimpleParser : IHasmTokenParser {
        public string Target { get; set; }

        public HasmSimpleParser(string target) {
            Target = target;
        }

        public bool CanParse(AssemblerState asm) {
            return asm.Stream.PeekWord() == Target;
        }

        public HasmToken Parse(AssemblerState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, $"invalid simple; expecting '{Target}'");
            }

            HasmStringStreamState state = asm.Stream.SaveState();
            return new HasmSimpleToken(state) {
                Value = asm.Stream.AdvanceWord()
            };
        }
    }
}
