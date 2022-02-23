using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
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
        /// <summary>
        /// The ID of the function.
        /// </summary>
        public uint Id { get; set; }
        /// <summary>
        /// The amount of parameters the function takes.
        /// </summary>
        public uint ParameterCount { get; set; }
        /// <summary>
        /// The amount of registers the function has (frame size).
        /// </summary>
        public uint RegisterCount { get; set; }
        /// <summary>
        /// The amount of symbols the function has (environment size).
        /// </summary>
        public uint Symbols { get; set; }
        /// <summary>
        /// True if the function is interpreted in strict mode (i.e. JS 'use strict')_, otherwise false.
        /// </summary>
        public bool Strict { get; set; }

        public HasmFunctionToken(HasmStringStreamState state) : base(state) { }
    }
}
