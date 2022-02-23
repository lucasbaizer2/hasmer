using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Assembler.Parser;

namespace HbcUtil.Assembler.Visitor {
    /// <summary>
    /// Represents an object being used to build a new Hermes bytecode file.
    /// </summary>
    public class HbcBuilder {
        private HasmTokenStream TokenStream;
        private IEnumerable<HasmToken> Tokens;

        private HbcBytecodeFormat Format;

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
            IEnumerator<HasmToken> enumerator = Tokens.GetEnumerator();
            while (enumerator.MoveNext()) {
                if (TokenStream.State.BytecodeFormat != null) {
                    Format = TokenStream.State.BytecodeFormat;
                    break;
                }
            }



            return new byte[0];
        }
    }
}
