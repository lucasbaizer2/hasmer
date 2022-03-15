using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class Literal : SyntaxNode {
        public PrimitiveValue Value { get; set; }

        public Literal(PrimitiveValue value) {
            Value = value;
        }

        public override void Write(SourceCodeBuilder builder) {
            if (Value.TypeCode == TypeCode.String) {
                builder.Write("'");
                builder.Write(StringEscape.Escape(Value.ToString()));
                builder.Write("'");
            } else {
                builder.Write(Value.ToString());
            }
        }
    }
}
