using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Parses a comment (starting with a '//') from the stream.
    /// </summary>
    public class HasmCommentParser : IHasmTokenParser {
        public bool CanParse(HasmReaderState asm) {
            return asm.Stream.Peek(2) == "//";
        }

        public HasmToken Parse(HasmReaderState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, "invalid comment");
            }

            while (true) {
                char c = asm.Stream.PeekChar();
                if (c == '\0') {
                    break;
                }
                asm.Stream.Advance(1);
                if (c == '\n') {
                    break;
                }
            }

            if (!asm.Stream.IsFinished) {
                asm.Stream.SkipWhitespace();
            }

            return null;
        }
    }
}
