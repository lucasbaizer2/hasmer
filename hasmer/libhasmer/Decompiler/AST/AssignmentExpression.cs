using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class AssignmentExpression : SyntaxNode {
        public string DeclarationKind { get; set; }
        public SyntaxNode Left { get; set; }
        public SyntaxNode Right { get; set; }
        public string Operator { get; set; }

        public override void Write(SourceCodeBuilder builder) {
            if (DeclarationKind != null) {
                builder.Write(DeclarationKind);
                builder.Write(" ");
            }

            Left.Write(builder);
            builder.Write(" ");
            builder.Write(Operator);
            builder.Write(" ");
            Right.Write(builder);
        }
    }
}
