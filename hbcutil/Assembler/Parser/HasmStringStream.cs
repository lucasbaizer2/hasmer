using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmStringStream {
        public int CurrentLine { get; set; }
        public int CurrentColumn { get; set; }
        public string[] Lines { get; set; }

        private string CurrentContent => Lines[CurrentLine].Substring(CurrentColumn);

        public bool IsFinished => CurrentLine == Lines.Length;

        public HasmStringStream(string hasm) {
            string lineSeparator = hasm.Contains("\r\n") ? "\r\n" : "\n";
            Lines = hasm.Split(lineSeparator).ToArray();
        }

        public string Peek(int length) {
            if (IsFinished) {
                throw new Exception("stream is finished");
            }
            if (length > CurrentContent.Length) {
                return null;
            }
            return CurrentContent.Substring(0, length);
        }

        public void SkipWhitespace() {
            while (Peek(1) == " ") {
                Advance(1);
            }
        }

        public string PeekWord() {
            int start = 0;
            while (Peek(start + 1).Trim() == "") {
                start++;
            }
            for (int i = 1; ; i++) {
                string peeked = Peek(start + i);
                if (peeked == null) { // end of line, return token
                    string previous = Peek(start + i - 1);
                    previous = previous.Substring(start);
                    return previous;
                }
                peeked = peeked.Substring(start);
                if (peeked[peeked.Length - 1] == ' ') {
                    return peeked.Substring(0, peeked.Length - 1);
                }
            }
        }

        public string AdvanceWord() {
            SkipWhitespace();
            string word = PeekWord();
            Advance(word.Length);
            return word;
        }

        public void Advance(int length) {
            if (IsFinished) {
                throw new Exception("stream is finished");
            }
            if (length > CurrentContent.Length) {
                throw new Exception("cannot advance beyond line length");
            }
            CurrentColumn += length;
            if (CurrentColumn == Lines[CurrentLine].Length) {
                CurrentLine++;
                CurrentColumn = 0;
            }
        }
    }
}
