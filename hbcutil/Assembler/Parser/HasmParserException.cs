using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmParserException : Exception {
        private HasmStringStream Stream;
        private string ErrorMessage;

        public override string Message => $"{ErrorMessage} at {Stream.CurrentLine + 1},{Stream.CurrentColumn}";

        public HasmParserException(HasmStringStream stream, string message) {
            Stream = stream;
            ErrorMessage = message;
        }
    }
}
