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

        public override void WriteDirect(SourceCodeBuilder builder) {
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

        public override void WriteDirect(SourceCodeBuilder builder) {
            if (Properties.Count == 0) {
                builder.Write("{}");
                return;
            }

            builder.Write("{");
            builder.AddIndent(1);
            builder.NewLine();

            for (int i = 0; i < Properties.Count; i++) {
                ObjectExpressionProperty property = Properties[i];

                property.Write(builder);
                if (i < Properties.Count - 1) {
                    builder.Write(",");
                }
                builder.NewLine();
            }

            builder.RemoveLastIndent();
            builder.AddIndent(-1);
            builder.Write("}");
        }
    }
}
