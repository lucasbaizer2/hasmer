using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a function definition token.
    /// </summary>
    public class HasmFunctionToken : HasmToken {
        /// <summary>
        /// The name of the function.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// The tokens in the body of the function.
        /// </summary>
        public List<HasmToken> Body { get; set; }

        public HasmFunctionToken(HasmStringStreamState state) : base(state) { }
    }
}
