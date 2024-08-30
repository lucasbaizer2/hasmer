using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents an 8-byte IEEE754 floating-point value.
    /// 
    /// This is directly equivalent to the "number" type in JavaScript.
    /// </summary>
    public class HasmNumberToken : HasmLiteralToken {
        /// <summary>
        /// The parsed value.
        /// </summary>
        public double Value { get; set; }

        public HasmNumberToken(HasmStringStreamState state) : base(state) { }
    }
}
