using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class MemberExpression : SyntaxNode {
        private static readonly Regex IdentifierRegex = new Regex(@"^([A-Za-z]|_|\$)([A-Za-z]|_|\$|[0-9])+$", RegexOptions.Compiled);

        public SyntaxNode Object { get; set; }
        public SyntaxNode Property { get; set; }
        public bool IsComputed { get; set; }

        private bool AutoCompute;

        public MemberExpression() : this(true) { }

        public MemberExpression(bool autoCompute) {
            AutoCompute = autoCompute;
        }

        public override void Write(SourceCodeBuilder builder) {
            if (AutoCompute) {
                if (Property is not Identifier ident) {
                    IsComputed = true;
                } else {
                    IsComputed = !IdentifierRegex.IsMatch(ident.Name);
                }
            }
            
            if (Object is Identifier red && red.IsRedundant) {
                // if the object is a redundant identifier, just write the property
                Property.Write(builder);
                return;
            }

            Object.Write(builder);
            if (IsComputed) {
                builder.Write("[");
                Property.Write(builder);
                builder.Write("]");
            } else {
                builder.Write(".");
                Property.Write(builder);
            }
        }
    }
}
