using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    /// <summary>
    /// Represents a token in the JavaScript syntax tree.
    /// </summary>
    public interface ISyntax {
        /// <summary>
        /// Writes the token as a string to the given <see cref="SourceCodeBuilder"/>.
        /// </summary>
        public void Write(SourceCodeBuilder builder);
    }
}
