using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public struct HasmStringStreamState {
        public int CurrentLine { get; set; }
        public int CurrentColumn { get; set; }
    }

    public enum HasmStringStreamWhitespaceMode {
        Remove,
        Keep
    }

    public class HasmStringStream {
        private const string WordCharacters = "ABCDEFGHIJKLMNOPQRSTUVQXYZabcdefghijklmnopqrstuvqxyz0123456789_";
        private const string OperatorCharacters = "<>[]()+-/*{},.";

        public int CurrentLine { get; set; }
        public int CurrentColumn { get; set; }
        public string[] Lines { get; set; }
        public HasmStringStreamWhitespaceMode WhitespaceMode { get; set; }

        private string CurrentContent => Lines[CurrentLine].Substring(CurrentColumn);

        public bool IsFinished => CurrentLine == Lines.Length;

        public HasmStringStream(string hasm) {
            string lineSeparator = hasm.Contains("\r\n") ? "\r\n" : "\n";
            Lines = hasm.Split(lineSeparator).ToArray();
            WhitespaceMode = HasmStringStreamWhitespaceMode.Remove;
        }

        private string Peek(int length) {
            if (IsFinished) {
                throw new Exception("asm.Stream is finished");
            }
            if (length > CurrentContent.Length) {
                return null;
            }
            return CurrentContent.Substring(0, length);
        }

        public void SkipWhitespace() {
            if (WhitespaceMode == HasmStringStreamWhitespaceMode.Remove) {
                while (Peek(1) == " ") {
                    Advance(1);
                }
            }
        }

        public string PeekCharacters(int length) {
            SkipWhitespace();
            return Peek(length);
        }

        public string AdvanceCharacters(int length) {
            string chars = PeekCharacters(length);
            Advance(chars.Length);
            return chars;
        }

        public string PeekOperator() {
            SkipWhitespace();
            string peeked = Peek(1);
            if (peeked == null) {
                return null;
            }
            if (!OperatorCharacters.Contains(peeked)) {
                return null;
            }
            return peeked;
        }

        public string AdvanceOperator() {
            string op = PeekOperator();
            Advance(op.Length);
            return op;
        }

        public string PeekWord() {
            SkipWhitespace();
            for (int i = 1; ; i++) {
                string peeked = Peek(i);
                if (peeked == null) { // end of line, return token
                    return Peek(i - 1);
                }
                if (!WordCharacters.Contains(peeked[peeked.Length - 1])) {
                    string word = peeked.Substring(0, peeked.Length - 1);
                    if (word == "") {
                        return null;
                    }
                    return word;
                }
            }
        }

        public string AdvanceWord() {
            string word = PeekWord();
            Advance(word.Length);
            return word;
        }

        public void Advance(int length) {
            if (IsFinished) {
                throw new Exception("asm.Stream is finished");
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

        public HasmStringStreamState SaveState() {
            return new HasmStringStreamState {
                CurrentLine = CurrentLine,
                CurrentColumn = CurrentColumn
            };
        }

        public void LoadState(HasmStringStreamState state) {
            CurrentLine = state.CurrentLine;
            CurrentColumn = state.CurrentColumn;
        }
    }
}
