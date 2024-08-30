using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Hasmer.Decompiler.AST {
    /// <summary>
    /// Represents a JavaScript identifier, such as a field name, class name, etc.
    /// </summary>
    public class Identifier : SyntaxNode {
        /// <summary>
        /// A regular expression which represents a valid JavaScript identifier.
        /// </summary>
        public static readonly Regex NamePattern = new Regex(@"^([A-Za-z]|_|\$)([A-Za-z]|_|\$|[0-9])*$", RegexOptions.Compiled);

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

        public override void WriteDirect(SourceCodeBuilder builder) {
            if (IsRedundant) {
                throw new Exception("cannot write redunant identifier");
            }
            builder.Write(Name);
        }

        private static string EscapeIdentifier(string ident) {
            return ident.Replace(@"\", @"\\").Replace("<", @"\<").Replace(">", @"\>");
        }

        public override string ToString() {
            if (Name.Length == 0) {
                return "<>";
            } else {
                if (Identifier.NamePattern.IsMatch(Name)) {
                    return $"<{Name}>";
                } else {
                    return $"<{EscapeIdentifier(Name)}>";
                }
            }
        }
    }
}
