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
    public class ProgramDefinition : SyntaxNode {
        public List<SyntaxNode> Tokens { get; set; }

        public ProgramDefinition() {
            Tokens = new List<SyntaxNode>();
        }

        public override void Write(SourceCodeBuilder builder) {
            foreach (SyntaxNode node in Tokens) {
                node.Write(builder);
                builder.NewLine();
                builder.NewLine();
            }
        }
    }
}
