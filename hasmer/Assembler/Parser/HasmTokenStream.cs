using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a stream of Hasm tokens parsed from a Hasm file.
    /// </summary>
    public class HasmTokenStream {
        /// <summary>
        /// The current state of the reader.
        /// </summary>
        public HasmReaderState State { get; set; }

        /// <summary>
        /// Creates a new HasmTokenStream from raw Hasm assembly.
        /// </summary>
        public HasmTokenStream(string hasm) {
            State = new HasmReaderState {
                Stream = new HasmStringStream(hasm)
            };
        }

        /// <summary>
        /// Creates a new HasmTokenStream from an existing state.
        /// </summary>
        public HasmTokenStream(HasmReaderState state) {
            State = state;
        }

        /// <summary>
        /// Gets an enumerator returning each next token, parsed from the Hasm file.
        /// </summary>
        public IEnumerable<HasmToken> ReadTokens() {
            while (!State.Stream.IsFinished) {
                if (State.Stream.Lines[State.Stream.CurrentLine].Trim() == "") {
                    State.Stream.CurrentLine++;
                    continue;
                }

                State.Stream.SkipWhitespace();

                List<IHasmTokenParser> tokenParsers = new List<IHasmTokenParser> {
                    IHasmTokenParser.DeclarationParser,
                    IHasmTokenParser.CommentParser
                };
                if (State.CurrentFunction != null) {
                    tokenParsers.Add(IHasmTokenParser.InstructionParser);
                }

                bool parsed = false;
                foreach (IHasmTokenParser parser in tokenParsers) {
                    HasmStringStreamState state = State.Stream.SaveState();
                    if (parser.CanParse(State)) {
                        State.Stream.LoadState(state);
                        HasmToken token;
                        try {
                            token = parser.Parse(State);
                        } catch (HasmParserException) {
                            throw;
                        } catch (Exception e) {
                            throw new HasmParserException(State.Stream, e);
                        }
                        if (token != null) { // tokens like comments can return null, just ignore them
                            yield return token;
                        }

                        if (State.BytecodeFormat == null) {
                            if (token is HasmVersionDeclarationToken ver) {
                                uint value = ver.Version.GetValueAsUInt32();
                                State.BytecodeFormat = ResourceManager.ReadEmbeddedResource<HbcBytecodeFormat>($"Bytecode{value}");
                            } else {
                                throw new HasmParserException(State.Stream, "expecting '.hasm' declaration");
                            }
                        }

                        parsed = true;
                        break;
                    } else {
                        State.Stream.LoadState(state);
                    }
                }

                State.Stream.SkipWhitespace();

                if (!parsed) {
                    throw new HasmParserException(State.Stream, "invalid statement");
                }
            }
        }
    }
}
