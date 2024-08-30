﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents an error in syntax during the parsing of a Hasm file.
    /// </summary>
    public class HasmParserException : Exception {
        /// <summary>
        /// The line of the invalid token.
        /// </summary>
        private int Line;
        /// <summary>
        /// The column of the invalid token, i.e. the invalid token's first character.
        /// </summary>
        private int Column;
        /// <summary>
        /// The message describing why the token could not be parsed.
        /// </summary>
        private string ErrorMessage;

        public override string Message => $"{ErrorMessage} at {Line},{Column}";

        /// <summary>
        /// Creates a new HasmParserException given the current stream and a message.
        /// </summary>
        public HasmParserException(HasmStringStream stream, string message) {
            Line = stream.CurrentLine + 1;
            Column = stream.CurrentColumn + 1;
            ErrorMessage = message;
        }

        /// <summary>
        /// Creates a new HasmParserException given a token and a message.
        /// </summary>
        public HasmParserException(HasmToken token, string message) {
            Line = token.Line + 1;
            Column = token.Column + 1;
            ErrorMessage = message;
        }

        /// <summary>
        /// Creates a new HasmParserException given a token and a message.
        /// </summary>
        public HasmParserException(HasmStringStreamState state, string message) {
            Line = state.Line + 1;
            Column = state.Column + 1;
            ErrorMessage = message;
        }

        /// <summary>
        /// Creates a new HasmParserException given the current stream and an exception.
        /// </summary>
        public HasmParserException(HasmStringStream stream, Exception e) : base(e.Message, e) {
            Line = stream.CurrentLine + 1;
            Column = stream.CurrentColumn + 1;
        }
    }
}
