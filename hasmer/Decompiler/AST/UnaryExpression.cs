using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class UnaryExpression : ISyntax {
        public string Operator { get; set; }
        public ISyntax Argument { get; set; }

        public void Write(SourceCodeBuilder builder) {
            builder.Write(Operator);
            builder.Write(" ");
            Argument.Write(builder);
        }
    }
}
