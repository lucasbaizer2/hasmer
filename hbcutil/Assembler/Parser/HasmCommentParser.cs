using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmCommentParser : IHasmTokenParser {
        public bool CanParse(AssemblerState asm) {
            return asm.Stream.PeekCharacters(1) == "#";
        }

        public HasmToken Parse(AssemblerState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, "invalid comment");
            }

            asm.Stream.CurrentLine++;
            asm.Stream.CurrentColumn = 0;
            if (!asm.Stream.IsFinished) {
                asm.Stream.SkipWhitespace();
            }

            return null;
        }
    }
}
