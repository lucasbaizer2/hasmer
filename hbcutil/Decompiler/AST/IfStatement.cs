using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Decompiler.AST {
    public class IfStatement : ISyntax {
        public ISyntax Test { get; set; }
        public ISyntax Consequent { get; set; }
        public ISyntax Alternate { get; set; }

        public void Write(SourceCodeBuilder builder) {
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
