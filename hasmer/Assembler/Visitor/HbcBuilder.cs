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
    public class HbcBuilder {
        private HasmTokenStream TokenStream;
        private IEnumerable<HasmToken> Tokens;

        /// <summary>
        /// The bytecode file that is being built.
        /// </summary>
        public HbcFileBuilder File { get; set; }

        /// <summary>
        /// Creates a new HbcBuilder given the tokens representing the Hasm file being assembled.
        /// </summary>
        public HbcBuilder(HasmTokenStream tokens) {
            TokenStream = tokens;
            Tokens = tokens.ReadTokens();
        }

        /// <summary>
        /// Parses the Hasm tokens and writes a Hermes bytecode file, serialized to a byte array.
        /// </summary>
        public byte[] Write() {
            HbcBytecodeFormat format = null;
            IEnumerator<HasmToken> enumerator = Tokens.GetEnumerator();
            while (enumerator.MoveNext()) {
                if (TokenStream.State.BytecodeFormat != null) {
                    format = TokenStream.State.BytecodeFormat;
                    break;
                }
            }

            if (format == null) {
                throw new Exception("empty Hasm file");
            }

            File = new HbcFileBuilder(format);
            return File.Build().Write();
        }
    }
}
