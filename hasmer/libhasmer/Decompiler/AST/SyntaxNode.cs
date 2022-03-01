using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hasmer.Decompiler.AST {
    /// <summary>
    /// Represents a token in the JavaScript syntax tree.
    /// </summary>
    public abstract class SyntaxNode {
#pragma warning disable IDE0051 // Remove unused private members
        /// <summary>
        /// Gets the type of the token (the class name of the implementation) as information when JSON serializing the token (used for debug).
        /// </summary>
        [JsonProperty("TokenType")]
        private string JsonTokenType => GetType().Name;
#pragma warning restore IDE0051 // Remove unused private members

        /// <summary>
        /// Writes the token as a string to the given <see cref="SourceCodeBuilder"/>.
        /// </summary>
        public abstract void Write(SourceCodeBuilder builder);
    }
}
