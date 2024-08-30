using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class MemberExpression : SyntaxNode {
        public SyntaxNode Object { get; set; }
        public SyntaxNode Property { get; set; }
        public bool IsComputed { get; set; }

        private bool AutoCompute;

        public MemberExpression() : this(true) { }

        public MemberExpression(bool autoCompute) {
            AutoCompute = autoCompute;
        }

        public SyntaxNode GetUltimateProperty() {
            SyntaxNode property = Property;
            while (true) {
                if (property is MemberExpression sub) {
                    property = sub.Property;
                } else {
                    return property;
                }
            }
        }

        public override void WriteDirect(SourceCodeBuilder builder) {
            if (AutoCompute) {
                if (Property is not Identifier ident) {
                    IsComputed = true;
                } else {
                    IsComputed = !Identifier.NamePattern.IsMatch(ident.Name);
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

        public override string ToString() {
            return $"{Object}.{Property}";
        }
    }
}
