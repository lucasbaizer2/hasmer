using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hasmer.Decompiler.AST {
    public class FunctionDeclaration : SyntaxNode {
        public Identifier Name { get; set; }
        public bool IsGenerator { get; set; }
        public bool IsAsync { get; set; }
        public List<Identifier> Parameters { get; set; }
        public BlockStatement Body { get; set; }
        public bool IsExpression { get; set; }

        [JsonIgnore]
        public HbcFuncHeader HbcHeader { get; set; }

        public FunctionDeclaration() {
            Parameters = new List<Identifier>();
        }

        public override void WriteDirect(SourceCodeBuilder builder) {
            if (IsAsync) {
                builder.Write("async ");
            }
            if (!IsExpression) {
                builder.Write("function");
            }
            if (IsGenerator) {
                builder.Write("*");
            }
            if (Name != null) {
                builder.Write(" ");
                Name.Write(builder);
            }
            builder.Write("(");
            for (int i = 0; i < Parameters.Count; i++) {
                Parameters[i].Write(builder);
                if (i < Parameters.Count - 1) {
                    builder.Write(", ");
                }
            }
            builder.Write(") ");
            Body.Write(builder);
        }
    }
}
