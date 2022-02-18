using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmToken {
        public HasmTokenType TokenType { get; set; }
        public string RawValue { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public List<HasmToken> Children { get; set; }

        public HasmToken(HasmStringStream stream, string raw) {
            RawValue = raw;

            if (stream.CurrentColumn == 0) { // last token ended the line
                Line = stream.CurrentLine - 1;
                Column = stream.Lines[Line].Length - raw.Length;
            } else {
                Line = stream.CurrentLine;
                Column = stream.CurrentColumn - raw.Length;
            }
        }

        public void Write(SourceCodeBuilder builder) {
            builder.Write(TokenType.ToString());
            builder.Write("('");
            builder.Write(RawValue);
            builder.Write("') at [");
            builder.Write(Line.ToString());
            builder.Write(", ");
            builder.Write(Column.ToString());
            builder.Write("]");

            if (Children != null) {
                builder.Write(" {");
                builder.AddIndent(1);
                builder.NewLine();

                foreach (HasmToken child in Children) {
                    child.Write(builder);
                    builder.Write(",");
                    builder.NewLine();
                }

                builder.Builder.Remove(builder.Builder.Length - 4, 4);
                builder.AddIndent(-1);
                builder.Write("}");
            }
        }
    }
}
