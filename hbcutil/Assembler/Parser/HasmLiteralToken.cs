using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public abstract class HasmLiteralToken : HasmToken {
        public HasmLiteralToken(HasmStringStreamState state) : base(state) { }
    }
}
