using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Assembler.Parser;

namespace HbcUtil.Assembler.Visitor {
    public class HbcBuilder {
        private HasmTokenStream TokenStream;
        private IEnumerable<HasmToken> Tokens;

        private HbcBytecodeFormat Format;

        public HbcBuilder(HasmTokenStream tokens) {
            TokenStream = tokens;
            Tokens = tokens.ReadTokens();
        }

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
