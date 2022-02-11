using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Decompile.AST {
    public class BinaryExpression : ISyntax {
        public ISyntax Left { get; set; }
        public ISyntax Right { get; set; }
        public string Operator { get; set; }

        public void Write(SourceCodeBuilder builder) {
            Left.Write(builder);
            builder.Write(" ");
            builder.Write(Operator);
            builder.Write(" ");
            Right.Write(builder);
        }
    }
}
