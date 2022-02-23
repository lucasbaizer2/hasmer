using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    /// <summary>
    /// Parses a comment (starting with a '#') from the stream.
    /// </summary>
    public class HasmCommentParser : IHasmTokenParser {
        public bool CanParse(HasmReaderState asm) {
            return asm.Stream.PeekCharacters(1) == "#";
        }

        public HasmToken Parse(HasmReaderState asm) {
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
