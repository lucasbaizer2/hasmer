using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class RegExpLiteral : SyntaxNode {
        public string Pattern { get; set; }
        public string Flags { get; set; }

        public RegExpLiteral(string pattern, string flags) {
            Pattern = pattern;
            Flags = flags;
        }

        public override void Write(SourceCodeBuilder builder) {
            builder.Write("/");
            builder.Write(Pattern);
            builder.Write("/");
            builder.Write(Flags);
        }
    }
}
