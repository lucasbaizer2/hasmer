using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class IfStatement : SyntaxNode {
        public SyntaxNode Test { get; set; }
        public SyntaxNode Consequent { get; set; }
        public SyntaxNode Alternate { get; set; }

        public override void Write(SourceCodeBuilder builder) {
            if (Alternate != null && Consequent is not BlockStatement && Alternate is not BlockStatement) {
                // if the consequent and alternate are both simple expressions,
                // then the if statement can be simplified to a ternary operation
                Test.Write(builder);
                builder.Write(" ? ");
                Consequent.Write(builder);
                builder.Write(" : ");
                Alternate.Write(builder);
                return;
            }

            builder.Write("if (");
            Test.Write(builder);
            builder.Write(") ");
            Consequent.Write(builder);

            if (Alternate != null) {
                builder.Write(" else ");
                Alternate.Write(builder);
            }
        }
    }
}
