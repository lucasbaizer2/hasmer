using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class CallExpression : ISyntax {
        public ISyntax Callee { get; set; }
        public List<ISyntax> Arguments { get; set; }
        public bool IsCalleeConstructor { get; set; }

        public CallExpression() {
            Arguments = new List<ISyntax>();
        }

        public void Write(SourceCodeBuilder builder) {
            if (IsCalleeConstructor) {
                builder.Write("new ");
            }
            Callee.Write(builder);
            builder.Write("(");
            for (int i = 0; i < Arguments.Count; i++) {
                Arguments[i].Write(builder);
                if (i < Arguments.Count - 1) {
                    builder.Write(", ");
                }
            }
            builder.Write(")");
        }
    }
}
