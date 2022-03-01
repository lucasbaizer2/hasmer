using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class BlockStatement : ISyntax {
        public List<ISyntax> Body { get; set; }

        public BlockStatement() {
            Body = new List<ISyntax>();
        }

        public void Write(SourceCodeBuilder builder) {
            builder.Write("{");
            builder.AddIndent(1);
            builder.NewLine();

            foreach (ISyntax syntax in Body) {
                syntax.Write(builder);
                builder.NewLine();
            }

            builder.RemoveLastIndent();
            builder.AddIndent(-1);
            builder.Write("}");
        }
    }
}
