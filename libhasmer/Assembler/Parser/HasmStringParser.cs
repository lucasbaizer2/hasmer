using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a string literal.
    /// </summary>
    public class HasmStringToken : HasmLiteralToken {
        /// <summary>
        /// The value of the string, without double quotes.
        /// </summary>
        public string Value { get; set; }

        public HasmStringToken(HasmStringStreamState state) : base(state) { }
    }

    /// <summary>
    /// Represents an identifier.
    /// </summary>
    public class HasmIdentifierToken : HasmLiteralToken {
        /// <summary>
        /// The name of the identifier, without the angled brackets.
        /// </summary>
        public string Value { get; set; }

        public HasmIdentifierToken(HasmStringStreamState state) : base(state) { }
    }

    /// <summary>
    /// Parses an escaped string surrounded by double quotes.
    /// </summary>
    public class HasmStringParser : IHasmTokenParser {
        enum ParserState {
            Normal,
            Escape
        }

        private HasmLiteralToken TryParse(HasmReaderState asm) {
            HasmStringStreamWhitespaceMode prevMode = asm.Stream.WhitespaceMode;
            asm.Stream.WhitespaceMode = HasmStringStreamWhitespaceMode.Keep;

            char c = asm.Stream.PeekChar();
            if (c == '\0' || c != '"' && c != '<') {
                return null;
            }
            HasmStringStreamState streamState = asm.Stream.SaveState();
            asm.Stream.Advance(1);

            StringBuilder res = new StringBuilder();
            ParserState parserState = ParserState.Normal;
            if (c == '"') {
                while ((c = asm.Stream.AdvanceChar()) != '\0') {
                    if (parserState == ParserState.Normal) {
                        if (c == '"') {
                            asm.Stream.WhitespaceMode = prevMode;
                            return new HasmStringToken(streamState) {
                                Value = res.ToString(),
                            };
                        } else if (c == '\\') {
                            parserState = ParserState.Escape;
                        } else {
                            res.Append(c);
                        }
                    } else if (parserState == ParserState.Escape) {
                        char m;
                        switch (c) {
                            case '"': m = '"'; break;
                            case '\\': m = '\\'; break;
                            case '0': m = '\0'; break;
                            case 'a': m = '\a'; break;
                            case 'b': m = '\b'; break;
                            case 'f': m = '\f'; break;
                            case 'n': m = '\n'; break;
                            case 'r': m = '\r'; break;
                            case 't': m = '\t'; break;
                            case 'v': m = '\v'; break;
                            case 'u':
                                string hex = asm.Stream.AdvanceCharacters(4);
                                if (hex == null) {
                                    asm.Stream.WhitespaceMode = prevMode;
                                    return null;
                                }
                                m = (char)Convert.ToInt32(hex, 16);
                                break;
                            default:
                                throw new Exception($"invalid string escape code: \\{c}");
                        }

                        parserState = ParserState.Normal;
                        res.Append(m);
                    }
                }
            } else if (c == '<') {
                while ((c = asm.Stream.AdvanceChar()) != '\0') {
                    if (parserState == ParserState.Normal) {
                        if (c == '>') {
                            asm.Stream.WhitespaceMode = prevMode;
                            return new HasmIdentifierToken(streamState) {
                                Value = res.ToString(),
                            };
                        } else if (c == '\\') {
                            parserState = ParserState.Escape;
                        } else {
                            res.Append(c);
                        }
                    } else if (parserState == ParserState.Escape) {
                        char m;
                        switch (c) {
                            case '<': m = '<'; break;
                            case '>': m = '>'; break;
                            case '\\': m = '\\'; break;
                            default:
                                throw new Exception($"invalid identifier escape code: \\{c}");
                        }

                        parserState = ParserState.Normal;
                        res.Append(m);
                    }
                }
            }

            asm.Stream.WhitespaceMode = prevMode;
            return null;
        }

        public bool CanParse(HasmReaderState asm) {
            HasmStringStreamState state = asm.Stream.SaveState();
            HasmLiteralToken s = TryParse(asm);
            asm.Stream.LoadState(state);
            return s != null;
        }

        public HasmToken Parse(HasmReaderState asm) {
            HasmStringStreamState state = asm.Stream.SaveState();
            HasmLiteralToken s = TryParse(asm);
            if (s == null) {
                throw new HasmParserException(asm.Stream, "invalid string literal or identifier");
            }

            return s;
        }
    }
}
