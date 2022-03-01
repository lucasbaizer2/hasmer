using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a class which can parse Hasm tokens from a Hasm file.
    /// </summary>
    public interface IHasmTokenParser {
        public static readonly IHasmTokenParser CommentParser = new HasmCommentParser();
        public static readonly IHasmTokenParser DeclarationParser = new HasmDeclarationParser();
        public static readonly IHasmTokenParser IntegerParser = new HasmIntegerParser();
        public static readonly IHasmTokenParser NumberParser = new HasmNumberParser();
        public static readonly IHasmTokenParser StringParser = new HasmStringParser();
        public static readonly IHasmTokenParser LabelParser = new HasmLabelParser();
        public static readonly IHasmTokenParser InstructionParser = new HasmInstructionParser();

        /// <summary>
        /// Returns true if a valid token can be parsed immediately, or false if one cannot.
        /// 
        /// This method should not affect the current stream's state.
        /// </summary>
        bool CanParse(HasmReaderState asm);

        /// <summary>
        /// Parses a token from the stream, affecting its state.
        /// </summary>
        HasmToken Parse(HasmReaderState asm);
    }
}
