using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    /// <summary>
    /// Represents the entire decompile program as a sequence of tokens.
    /// This is the root token of the output.
    /// </summary>
    public class ProgramDefinition : BlockStatement {
        public ProgramDefinition() : base() {
        }

        public override void WriteDirect(SourceCodeBuilder builder) {
            foreach (SyntaxNode node in Body) {
                if (node is EmptyExpression) {
                    continue;
                }

                node.Write(builder);
                if (node is not IfStatement && node is not FunctionDeclaration) {
                    builder.Write(";");
                }
                builder.NewLine();
            }
        }
    }
}
