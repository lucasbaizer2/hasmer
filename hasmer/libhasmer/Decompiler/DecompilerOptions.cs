using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler {
    /// <summary>
    /// Represents the options to be used when decompiling.
    /// </summary>
    public struct DecompilerOptions {
        /// <summary>
        /// True if the class prototype should be omitted as the first parameter to a construction invocation, otherwise false.
        /// <br />
        /// <example>
        /// As an example, when this is true:
        /// <br />
        /// <code>
        /// new Promise(handler)
        /// </code>
        /// When this is false:
        /// <br />
        /// <code>
        /// new Promise(Promise.prototype, handler)
        /// </code>
        /// </example>
        /// </summary>
        public bool OmitPrototypeFromConstructorInvocation { get; set; }

        /// <summary>
        /// True if the "this" parameter should be omitted as the first parameter to a function invocation, otherwise false.
        /// <br />
        /// <example>
        /// As an example, when this is true:
        /// <br />
        /// <code>
        /// Promise.resolve(false)
        /// </code>
        /// When this is false:
        /// <br />
        /// <code>
        /// Promise.resolve(Promise, false)
        /// </code>
        /// </example>
        /// </summary>
        public bool OmitThisFromFunctionInvocation { get; set; }

        /// <summary>
        /// True if the "global" identifier should be omitted as a prefix to global field references.
        /// <br />
        /// <example>
        /// As an example, when this is true:
        /// <br />
        /// <code>
        /// Promise.resolve(false)
        /// </code>
        /// When this is false:
        /// <br />
        /// <code>
        /// global.Promise.resolve(false)
        /// </code>
        /// </example>
        /// </summary>
        public bool OmitExplicitGlobal { get; set; }
    }
}
