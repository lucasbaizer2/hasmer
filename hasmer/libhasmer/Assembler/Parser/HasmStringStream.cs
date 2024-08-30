using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    public struct HasmStringStreamState {
        public int Offset { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
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
        private const string WordCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_$";

        /// <summary>
        /// The valid characters for operators, returned by <see cref="PeekOperator"/>
        /// </summary>
        private const string OperatorCharacters = "<>[]()+-/*{},.";

        /// <summary>
        /// The current offset of the file the stream is currently at.
        /// </summary>
        public int Cursor { get; set; }

        public int CurrentLine {
            get {
                if (Cursor == 0) {
                    return 0;
                }

                // returns the amount of new lines between the start and the cursor
                string s = Source.Substring(0, Cursor);
                return s.Count(c => c == '\n');
            }
        }

        public int CurrentColumn {
            get {
                if (Cursor == 0) {
                    return 0;
                }
                
                // returns the amount of characters between the cursor and the last new line
                int i = Source.LastIndexOf('\n', Cursor);
                return Cursor - i;
            }
        }

        /// Contains each individual line of the Hasm file.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The method that the stream should use to treat whitespace.
        /// </summary>
        public HasmStringStreamWhitespaceMode WhitespaceMode { get; set; }

        /// <summary>
        /// true if the stream is finished (i.e. all data has been read from it), otherwise false.
        /// </summary>
        public bool IsFinished => Cursor == Source.Length;

        /// <summary>
        /// Returns a new HasmStringStream given the raw Hasm assembly.
        /// </summary>
        public HasmStringStream(string hasm) {
            Source = hasm;
            WhitespaceMode = HasmStringStreamWhitespaceMode.Remove;
        }

        /// <summary>
        /// Returns *length* characters of the <see cref="Source" /> starting at the <see cref="Cursor" />.
        /// If the requested length is the beyond the amount of characters in the Source, null is returned.
        /// </summary>
        public string Peek(int length) {
            SkipWhitespace();

            int r = Cursor + length;
            if (r < 0 || r > Source.Length) {
                return null;
            }
            return Source.Substring(Cursor, length);
        }

        public char PeekChar() {
            SkipWhitespace();
            
            if (IsFinished) {
                return '\0';
            }
            return Source[Cursor];
        }

        /// <summary>
        /// Skips all whitespace until there is a character which is not whitespace or the end of the input.
        /// </summary>
        public void SkipWhitespace() {
            if (WhitespaceMode == HasmStringStreamWhitespaceMode.Remove) {
                while (!IsFinished) {
                    char c = Source[Cursor];
                    if (c == '\0') {
                        break;
                    }
                    if (!char.IsWhiteSpace(c)) {
                        break;
                    }
                    Cursor++;
                }
            }
        }

        /// <summary>
        /// Advances the stream by *length* characters.
        /// </summary>
        public string AdvanceCharacters(int length) {
            string chars = Peek(length);
            if (chars == null) {
                return null;
            }
            Advance(chars.Length);
            return chars;
        }

        /// <summary>
        /// Peeks a single-character operator from the stream.
        /// <br />
        /// If the stream has ended, or the operator is not contained within <see cref="OperatorCharacters"/>, null is returned.
        /// </summary>
        public char PeekOperator() {
            char peeked = PeekChar();
            if (peeked == '\0') {
                return '\0';
            }
            if (!OperatorCharacters.Contains(peeked)) {
                return '\0';
            }
            return peeked;
        }

        /// <summary>
        /// Advances the stream beyond the current operator.
        /// </summary>
        public char AdvanceOperator() {
            char op = PeekOperator();
            if (op == '\0') {
                return '\0';
            }
            AdvanceChar();
            return op;
        }

        /// <summary>
        /// Advances the stream beyond the current character.
        /// </summary>
        public char AdvanceChar() {
            char c = PeekChar();
            if (c == '\0') {
                return '\0';
            }
            Advance(1);
            return c;
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
            int cursor = Cursor;
            HasmStringStreamWhitespaceMode wm = WhitespaceMode;

            WhitespaceMode = HasmStringStreamWhitespaceMode.Keep;

            StringBuilder builder = new StringBuilder();
            for (int i = 1; ; i++) {
                char peeked = AdvanceChar();
                if (peeked == '\0') { // end of input, return token
                    break;
                }
                if (!WordCharacters.Contains(peeked)) { // end of word, return token
                    break;
                }
                builder.Append(peeked);
            }
            
            Cursor = cursor;
            WhitespaceMode = wm;

            string word = builder.ToString();
            if (word == "") {
                return null;
            }
            return word;
        }

        /// <summary>
        /// Advances the stream beyond the current word.
        /// </summary>
        /// <returns>the word that was read</returns>
        public string AdvanceWord() {
            string word = PeekWord();
            if (word == null) {
                return null;
            }
            Advance(word.Length);
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
            int r = Cursor + length;
            if (r < 0 || r > Source.Length) {
                throw new IndexOutOfRangeException($"new cursor position out of bounds: {r}");
            }

            Cursor = r;

            SkipWhitespace();
        }

        /// <summary>
        /// Gets the current state of the stream. 
        /// </summary>
        public HasmStringStreamState SaveState() {
            return new HasmStringStreamState {
                Offset = Cursor,
                Line = CurrentLine,
                Column = CurrentColumn,
            };
        }

        /// <summary>
        /// Sets the current state of the stream to *state*, which is generally used as the value returned by <see cref="SaveState"/>.
        /// </summary>
        public void LoadState(HasmStringStreamState state) {
            int cursor = state.Offset;
            if (cursor < 0 || cursor > Source.Length) {
                throw new IndexOutOfRangeException($"loaded cursor position out of bounds: {cursor}");
            }
            Cursor = cursor;
        }
    }
}
