using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class ObjectExpressionProperty : SyntaxNode {
        public SyntaxNode Key { get; set; }
        public SyntaxNode Value { get; set; }
        public bool IsComputed { get; set; }

        public override void Write(SourceCodeBuilder builder) {
            if (IsComputed) {
                builder.Write("[");
            }
            Key.Write(builder);
            if (IsComputed) {
                builder.Write("]");
            }
            builder.Write(": ");
            Value.Write(builder);
        }
    }

    public class ObjectExpression : SyntaxNode {
        public List<ObjectExpressionProperty> Properties { get; set; }

        public ObjectExpression() {
            Properties = new List<ObjectExpressionProperty>();
        }

        public override void Write(SourceCodeBuilder builder) {
            if (Properties.Count == 0) {
                builder.Write("{}");
                return;
            }

            builder.Write("{");
            builder.AddIndent(1);
            builder.NewLine();

            foreach (ObjectExpressionProperty property in Properties) {
                property.Write(builder);
                builder.Write(",");
                builder.NewLine();
            }

            builder.RemoveLastIndent();
            builder.AddIndent(-1);
            builder.Write("}");
        }
    }
}
