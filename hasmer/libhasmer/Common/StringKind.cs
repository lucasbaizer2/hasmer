using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer {
    /// <summary>
    /// Used for determining whether a string is an identifier or a string literal.
    /// </summary>
    [Flags]
    public enum StringKind {
        /// <summary>
        /// The string is a string literal, e.g. a string in quotes: `var s = "Hello, World!"`.
        /// "Hello, World!" would be encoded as a string literal, i.e. this flag.
        /// </summary>
        String = 0 << 31,
        /// <summary>
        /// The string is an identifier, e.g. the name of a function.
        /// </summary>
        Identifier = 1 << 31
    }
}
