using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class UnaryExpression : SyntaxNode {
        public string Operator { get; set; }
        public SyntaxNode Argument { get; set; }

        public override void Write(SourceCodeBuilder builder) {
            builder.Write(Operator);
            if (Operator != "!") {
                builder.Write(" ");
            }
            builder.Write("(");
            Argument.Write(builder);
            builder.Write(")");
        }
    }
}
