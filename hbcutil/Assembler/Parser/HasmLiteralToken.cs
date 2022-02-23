using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    /// <summary>
    /// Represents a literal value, such as a numeric or string value.
    /// </summary>
    public abstract class HasmLiteralToken : HasmToken {
        public HasmLiteralToken(HasmStringStreamState state) : base(state) { }
    }
}
