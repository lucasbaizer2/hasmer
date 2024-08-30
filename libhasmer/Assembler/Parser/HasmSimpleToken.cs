using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a "simple" value, generally a constant identifier (e.g. "true" or "false").
    /// </summary>
    public class HasmSimpleToken : HasmLiteralToken {
        /// <summary>
        /// The simple value.
        /// </summary>
        public string Value { get; set; }

        public HasmSimpleToken(HasmStringStreamState state) : base(state) { }
    }
}
