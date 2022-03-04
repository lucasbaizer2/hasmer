using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Assembler.Parser;

namespace Hasmer.Assembler.Visitor {
    /// <summary>
    /// Represents an object being used to build a new Hermes bytecode file.
    /// </summary>
    public class HasmHeaderReader {
        private HasmTokenStream TokenStream;
        private IEnumerable<HasmToken> Tokens;

        /// <summary>
        /// The bytecode format being used for the file.
        /// </summary>
        public HbcBytecodeFormat Format { get; set; }

        /// <summary>
        /// True if the bytecode is being written as-is (i.e. literally), false if the bytecode should be optimized into variants.
        /// See <see cref="HbcDisassembler.IsExact"/> for more information
        /// </summary>
        public bool IsExact { get; set; }

        /// <summary>
        /// Creates a new HbcBuilder given the tokens representing the Hasm file being assembled.
        /// </summary>
        public HasmHeaderReader(HasmTokenStream tokens) {
            TokenStream = tokens;
            Tokens = tokens.ReadTokens();
        }

        /// <summary>
        /// Reads 
        /// </summary>
        public void Read() {
            IEnumerator<HasmToken> enumerator = Tokens.GetEnumerator();
            while (enumerator.MoveNext()) {
                if (TokenStream.State.BytecodeFormat != null) {
                    Format = TokenStream.State.BytecodeFormat;
                    IsExact = TokenStream.State.IsExact;

                    break;
                }
            }

            if (Format == null) {
                throw new Exception("empty Hasm file");
            }
        }
    }
}
