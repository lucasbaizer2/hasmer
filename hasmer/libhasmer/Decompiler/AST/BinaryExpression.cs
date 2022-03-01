using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class BinaryExpression : SyntaxNode {
        public SyntaxNode Left { get; set; }
        public SyntaxNode Right { get; set; }
        public string Operator { get; set; }

        public override void Write(SourceCodeBuilder builder) {
            if (Left is BinaryExpression) {
                builder.Write("(");
                Left.Write(builder);
                builder.Write(")");
            } else {
                Left.Write(builder);
            }
            builder.Write(" ");
            builder.Write(Operator);
            builder.Write(" ");
            if (Right is BinaryExpression) {
                builder.Write("(");
                Right.Write(builder);
                builder.Write(")");
            } else {
                Right.Write(builder);
            }
        }
    }
}
