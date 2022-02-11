using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Decompile.AST {
    public class IfStatement : ISyntax {
        public ISyntax Test { get; set; }
        public BlockStatement Consequent { get; set; }
        public ISyntax Alternate { get; set; }

        public void Write(SourceCodeBuilder builder) {
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
