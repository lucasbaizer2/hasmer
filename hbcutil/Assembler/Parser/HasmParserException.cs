using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmParserException : Exception {
        private int Line;
        private int Column;
        private string ErrorMessage;

        public override string Message => $"{ErrorMessage} at {Line},{Column}";

        public HasmParserException(HasmStringStream stream, string message) {
            Line = stream.CurrentLine + 1;
            Column = stream.CurrentColumn + 1;
            ErrorMessage = message;
        }

        public HasmParserException(HasmStringStream stream, Exception e) : base(e.Message, e) {
            Line = stream.CurrentLine + 1;
            Column = stream.CurrentColumn + 1;
        }

        public HasmParserException(int line, int col, string message) {
            Line = line + 1;
            Column = col + 1;
            ErrorMessage = message;
        }
    }
}
