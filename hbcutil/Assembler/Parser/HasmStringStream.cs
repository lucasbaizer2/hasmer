using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    /// <summary>
    /// Represents the state of a <see cref="HasmStringStream"/>.
    /// </summary>
    public struct HasmStringStreamState {
        /// <summary>
        /// The line the stream was on.
        /// </summary>
        public int CurrentLine { get; set; }
        /// <summary>
        /// The column the stream was on.
        /// </summary>
        public int CurrentColumn { get; set; }
    }

    /// <summary>
    /// Represents how the stream should treat whitespace.
    /// </summary>
    public enum HasmStringStreamWhitespaceMode {
        /// <summary>
        /// Removes all whitespace between tokens.
        /// </summary>
        Remove,
        /// <summary>
        /// Keeps all whitespace between tokens.
        /// </summary>
        Keep
    }

    /// <summary>
    /// Represents a stream that parses a Hasm assembly file.
    /// </summary>
    public class HasmStringStream {
        /// <summary>
        /// The characters that are valid in a word, returned by <see cref="PeekWord"/>.
        /// </summary>
        private const string WordCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
        /// <summary>
        /// The valid characters for operators, returned by <see cref="PeekOperator"/>
        /// </summary>
        private const string OperatorCharacters = "<>[]()+-/*{},.";

        /// <summary>
        /// The current line of the file the stream is on.
        /// </summary>
        public int CurrentLine { get; set; }
        /// <summary>
        /// The current column of the current line of the file the stream is on.
        /// </summary>
        public int CurrentColumn { get; set; }
        /// <summary>
        /// Contains each individual line of the Hasm file.
        /// </summary>
        public string[] Lines { get; set; }
        /// <summary>
        /// The method that the stream should use to treat whitespace.
        /// </summary>
        public HasmStringStreamWhitespaceMode WhitespaceMode { get; set; }

        /// <summary>
        /// The content of the current line, starting at the current column and ending at the end of the line.
        /// </summary>
        private string CurrentContent => Lines[CurrentLine].Substring(CurrentColumn);

        /// <summary>
        /// true if the stream is finished (i.e. all data has been read from it), otherwise false.
        /// </summary>
        public bool IsFinished => CurrentLine == Lines.Length;

        /// <summary>
        /// Returns a new HasmStringStream given the raw Hasm assembly.
        /// </summary>
        public HasmStringStream(string hasm) {
            string lineSeparator = hasm.Contains("\r\n") ? "\r\n" : "\n";
            Lines = hasm.Split(lineSeparator).ToArray();
            WhitespaceMode = HasmStringStreamWhitespaceMode.Remove;
        }

        /// <summary>
        /// Returns *length* characters of the <see cref="CurrentContent"/>.
        /// <br />
        /// If the requested length is the beyond the amount of characters in the CurrentContent, null is returned.
        /// <br />
        /// If the stream is finished, an exception is thrown.
        /// </summary>
        private string Peek(int length) {
            if (IsFinished) {
                throw new Exception("asm.Stream is finished");
            }
            if (length > CurrentContent.Length) {
                return null;
            }
            return CurrentContent.Substring(0, length);
        }

        /// <summary>
        /// Skips all whitespace starting at the current column until there is a character which is not whitespace.
        /// <br />
        /// This will not advance the stream to another line.
        /// </summary>
        public void SkipWhitespace() {
            if (WhitespaceMode == HasmStringStreamWhitespaceMode.Remove) {
                while (Peek(1) == " ") {
                    Advance(1);
                }
            }
        }

        /// <summary>
        /// Peeks *length* arbitray characters from the stream.
        /// </summary>
        public string PeekCharacters(int length) {
            SkipWhitespace();
            return Peek(length);
        }

        /// <summary>
        /// Advances the stream by *length* characters.
        /// </summary>
        public string AdvanceCharacters(int length) {
            string chars = PeekCharacters(length);
            Advance(chars.Length);
            SkipWhitespace();
            return chars;
        }

        /// <summary>
        /// Peeks a single-character operator from the stream.
        /// <br />
        /// If the stream has ended, or the operator is not contained within <see cref="OperatorCharacters"/>, null is returned.
        /// </summary>
        public string PeekOperator() {
            SkipWhitespace();
            string peeked = Peek(1);
            if (peeked == null) {
                return null;
            }
            if (!OperatorCharacters.Contains(peeked)) {
                return null;
            }
            SkipWhitespace();
            return peeked;
        }

        /// <summary>
        /// Advances the stream beyond the current operator.
        /// </summary>
        public string AdvanceOperator() {
            string op = PeekOperator();
            Advance(op.Length);
            SkipWhitespace();
            return op;
        }

        /// <summary>
        /// Peeks a word.
        /// A "word" is defined as an arbitrary length of characters, read until either the end of the current line or the presence of a character that is not contained by <see cref="WordCharacters"/>.
        /// <example>
        /// <code>
        /// line = "hello my na[jeff]me is" <br />
        /// AdvanceWord() = hello <br />
        /// AdvanceWord() = my <br />
        /// AdvanceWord() = na <br />
        /// AdvanceOperator() // skip the [ <br />
        /// AdvanceWord() = jeff <br />
        /// AdvanceOperator() // skip the ] <br />
        /// AdvanceWord() = me <br />
        /// AdvanceWord() = is
        /// </code>
        /// <see cref="AdvanceWord"/> <br />
        /// <see cref="AdvanceOperator"/>
        /// </example>
        /// </summary>
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
                    SkipWhitespace();
                    return word;
                }
            }
        }

        /// <summary>
        /// Advances the stream beyond the current word.
        /// </summary>
        /// <returns>the word that was read</returns>
        public string AdvanceWord() {
            string word = PeekWord();
            Advance(word.Length);
            SkipWhitespace();
            return word;
        }

        /// <summary>
        /// Advances the stream by *length* characters.
        /// 
        /// length cannot be greater than the length of the <see cref="CurrentContent"/>.
        /// 
        /// If length advances the stream to the end of the current line, the stream goes to the next line and sets the current column to zero.
        /// </summary>
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

        /// <summary>
        /// Gets the current state of the stream. 
        /// </summary>
        public HasmStringStreamState SaveState() {
            return new HasmStringStreamState {
                CurrentLine = CurrentLine,
                CurrentColumn = CurrentColumn
            };
        }

        /// <summary>
        /// Sets the current state of the stream to *state*, which is generally used as the value returned by <see cref="SaveState"/>.
        /// </summary>
        public void LoadState(HasmStringStreamState state) {
            CurrentLine = state.CurrentLine;
            CurrentColumn = state.CurrentColumn;
        }
    }
}
