using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmTokenStream {
        public static readonly IHasmTokenParser DeclarationParser = new HasmDeclarationParser();
        public static readonly IHasmTokenParser HasmIntegerParser = new HasmIntegerParser();

        private static readonly IHasmTokenParser[] TokenParsers = new IHasmTokenParser[] {
            DeclarationParser
        };

        private HasmStringStream Stream;

        public HasmTokenStream(string hasm) {
            Stream = new HasmStringStream(hasm);
        }

        public IEnumerable<HasmToken> ReadTokens() {
            while (!Stream.IsFinished) {
                if (Stream.Lines[Stream.CurrentLine].Trim() == "") {
                    Stream.CurrentLine++;
                    continue;
                }

                bool parsed = false;
                foreach (IHasmTokenParser parser in TokenParsers) {
                    if (parser.CanParse(Stream)) {
                        yield return parser.Parse(Stream);
                        parsed = true;
                        break;
                    }
                }

                if (!parsed) {
                    throw new HasmParserException(Stream, "invalid statement");
                }
            }
        }
    }
}
