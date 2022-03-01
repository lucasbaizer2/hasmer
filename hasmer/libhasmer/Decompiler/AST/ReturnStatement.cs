using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class ReturnStatement : SyntaxNode {
        public SyntaxNode Argument { get; set; }

        public override void Write(SourceCodeBuilder builder) {
            builder.Write("return ");
            Argument.Write(builder);
        }
    }
}
