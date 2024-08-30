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
