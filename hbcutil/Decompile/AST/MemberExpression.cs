using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HbcUtil.Decompile.AST {
    public class MemberExpression : ISyntax {
        private static readonly Regex IdentifierRegex = new Regex(@"^([A-Za-z]|_|\$)([A-Za-z]|_|\$|[0-9])+$", RegexOptions.Compiled);

        public ISyntax Object { get; set; }
        public ISyntax Property { get; set; }
        public bool IsComputed { get; set; }

        private bool AutoCompute;

        public MemberExpression() : this(true) { }

        public MemberExpression(bool autoCompute) {
            AutoCompute = autoCompute;
        }

        public void Write(SourceCodeBuilder builder) {
            if (AutoCompute) {
                if (Property is not Identifier ident) {
                    IsComputed = true;
                } else {
                    IsComputed = !IdentifierRegex.IsMatch(ident.Name);
                }
            }

            Object.Write(builder);
            if (IsComputed) {
                builder.Write("[");
                Property.Write(builder);
                builder.Write("]");
            } else {
                builder.Write(".");
                Property.Write(builder);
            }
        }
    }
}
