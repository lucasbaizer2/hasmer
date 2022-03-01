using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    /// <summary>
    /// Represents an empty token in the syntax tree.
    /// This is commonly used a placeholder during decompilation.
    /// </summary>
    public class EmptyExpression : ISyntax {
        public void Write(SourceCodeBuilder builder) {
            // no-op
        }
    }
}
