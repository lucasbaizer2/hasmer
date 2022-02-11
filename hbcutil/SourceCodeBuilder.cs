using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil {
    public class SourceCodeBuilder {
        public StringBuilder Builder { get; }

        private int IndentationLevel = 0;
        private string IndentationCharacter;

        public SourceCodeBuilder(string indentationCharacter) {
            IndentationCharacter = indentationCharacter;
            Builder = new StringBuilder();
        }

        public void NewLine() {
            Builder.AppendLine();
            if (IndentationLevel > 0) {
                Builder.Append(string.Concat(Enumerable.Repeat(IndentationCharacter, IndentationLevel)));
            }
        }

        public void Write(string code) {
            Builder.Append(code);
        }

        public void AddIndent(int amount) {
            IndentationLevel += amount;
        }

        /*
        public void Emit(SourceCodeBuilder cls) {
            string[] lines = cls.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines) {
                Emit(line.TrimStart());
            }
        }
        */

        public override string ToString() {
            return Builder.ToString();
        }
    }
}
