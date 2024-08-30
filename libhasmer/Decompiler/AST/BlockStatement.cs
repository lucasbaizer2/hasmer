using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class BlockStatement : SyntaxNode {
        public List<SyntaxNode> Body { get; set; }

        public BlockStatement() {
            Body = new List<SyntaxNode>();
        }

        public void WriteResult(uint register, SyntaxNode ast) {
            Body.Add(new AssignmentExpression {
                Left = new Identifier($"r{register}"),
                Right = ast,
                Operator = "="
            });
        }

        public override void WriteDirect(SourceCodeBuilder builder) {
            builder.Write("{");
            builder.AddIndent(1);
            builder.NewLine();

            foreach (SyntaxNode syntax in Body) {
                if (syntax is EmptyExpression) {
                    continue;
                }

                syntax.Write(builder);
                if (syntax is not IfStatement && syntax is not FunctionDeclaration) {
                    builder.Write(";");
                }
                builder.NewLine();
            }

            builder.RemoveLastIndent();
            builder.AddIndent(-1);
            builder.Write("}");
        }
    }
}
