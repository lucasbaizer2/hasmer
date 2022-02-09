using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil {
    public class CodeBuilder {
        private StringBuilder Builder = new StringBuilder();
        private int IndentationLevel = 0;
        private string IndentationCharacter;

        public CodeBuilder(string indentationCharacter) {
            IndentationCharacter = indentationCharacter;
        }

        public void Emit(string code) {
            if (code.StartsWith("}")) {
                IndentationLevel--;
            }

            if (IndentationLevel > 0) {
                Builder.Append(string.Concat(Enumerable.Repeat(IndentationCharacter, IndentationLevel)));
            }

            Builder.Append(code);
            Builder.Append("\r\n");

            if (code.EndsWith("{")) {
                IndentationLevel++;
            }
        }

        public void Emit(CodeBuilder cls) {
            string[] lines = cls.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines) {
                Emit(line.TrimStart());
            }
        }

        public override string ToString() {
            return Builder.ToString();
        }
    }
}
