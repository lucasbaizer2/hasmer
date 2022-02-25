using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class AssignmentExpression : ISyntax {
        public string DeclarationKind { get; set; }
        public ISyntax Left { get; set; }
        public ISyntax Right { get; set; }
        public string Operator { get; set; }

        public void Write(SourceCodeBuilder builder) {
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
