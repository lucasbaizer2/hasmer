using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmIntegerParser : IHasmTokenParser {
        public bool CanParse(HasmStringStream stream) {
            string word = stream.PeekWord();
            if (word == null) {
                return false;
            }

            return int.TryParse(word, out int _);
        }

        public HasmToken Parse(HasmStringStream stream) {
            if (!CanParse(stream)) {
                throw new HasmParserException(stream, "invalid integer");
            }

            string word = stream.AdvanceWord();
            return new HasmToken(stream, word) {
                TokenType = HasmTokenType.Integer
            };
        }
    }
}
