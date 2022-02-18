using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmDeclarationParser : IHasmTokenParser {
        public bool CanParse(HasmStringStream stream) {
            string word = stream.PeekWord();
            if (word == null) {
                return false;
            }

            return word == ".hasm" || word == ".data";
        }

        public HasmToken Parse(HasmStringStream stream) {
            string word = stream.AdvanceWord();
            if (word == ".hasm") {
                return new HasmToken(stream, ".hasm") {
                    TokenType = HasmTokenType.DeclarationVersion,
                    Children = new List<HasmToken>() {
                        HasmTokenStream.HasmIntegerParser.Parse(stream)
                    }
                };
            } else if (word == ".data") {
                // TODO
                return new HasmToken(stream, ".data") {
                    TokenType = HasmTokenType.DeclarationData,
                    Children = new List<HasmToken>() {
                        HasmTokenStream.HasmIntegerParser.Parse(stream)
                    }
                };
            }

            return null;
        }
    }
}
