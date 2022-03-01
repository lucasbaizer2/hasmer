using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents the state of a Hasm reader, especially a <see cref="HasmTokenStream"/>.
    /// </summary>
    public class HasmReaderState {
        /// <summary>
        /// The bytecode format of the Hasm file, used for both determining the format of instructions in the Hasm source and the version of the bytecode output.
        /// </summary>
        public HbcBytecodeFormat BytecodeFormat { get; set; }

        /// <summary>
        /// The stream of tokens from the Hasm file.
        /// </summary>
        public HasmStringStream Stream { get; set; }

        /// <summary>
        /// The current function being parsed. Otherwise, if the stream is outside the context of a function, this value is null.
        /// </summary>
        public HasmFunctionToken CurrentFunction { get; set; }

        /// <summary>
        /// Whether the bytecode instructions should be interpretered literally or not.
        /// See <see cref="HbcDisassembler.IsExact"/> for more information.
        /// </summary>
        public bool IsExact { get; set; }
    }
}
