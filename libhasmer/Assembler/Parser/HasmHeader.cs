
namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a ".hasm" header declaration.
    /// </summary>
    public class HasmHeader {
        /// <summary>
        /// The token which declares the version of the Hasm bytecode.
        /// </summary>
        public HasmIntegerToken Version { get; set; }

        /// <summary>
        /// True if the version declaration specified that the instructions should be interpretered literally,
        /// and not as abstracted forms. See <see cref="HbcDisassembler.IsExact"/> for more information.
        /// </summary>
        public bool IsExact { get; set; }
    }
}
