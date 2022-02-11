using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Decompile.AST {
    public class ClassDeclaration : ISyntax {
        public Identifier Name { get; set; }
        public Identifier SuperClass { get; set; }
        public List<MethodDefinition> Body { get; set; }

        public ClassDeclaration() {
            Body = new List<MethodDefinition>();
        }

        public void Write(SourceCodeBuilder builder) {
            builder.Write("class ");
            Name.Write(builder);

            if (SuperClass != null) {
                builder.Write(" extends ");
                SuperClass.Write(builder);
            }

            builder.Write(" {");
            builder.AddIndent(1);
            builder.NewLine();

            foreach (MethodDefinition method in Body) {
                method.Write(builder);
                builder.NewLine();
                builder.NewLine();
            }

            builder.AddIndent(-1);
            builder.NewLine();
            builder.Write("}");
        }
    }
}
