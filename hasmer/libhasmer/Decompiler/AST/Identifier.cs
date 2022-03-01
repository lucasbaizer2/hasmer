using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    /// <summary>
    /// Represents a JavaScript identifier, such as a field name, class name, etc.
    /// </summary>
    public class Identifier : SyntaxNode {
        /// <summary>
        /// The name of the identifier.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents whether the identifier is redudant and should not be written to the source tree.
        /// See <see cref="DecompilerOptions.OmitExplicitGlobal"/>.
        /// </summary>
        public bool IsRedundant { get; set; }

        /// <summary>
        /// Creates a new Identifier, given its name.
        /// </summary>
        public Identifier(string name) {
            Name = name;
        }

        public override void Write(SourceCodeBuilder builder) {
            if (IsRedundant) {
                throw new Exception("cannot write redunant identifier");
            }
            builder.Write(Name);
        }
    }
}
