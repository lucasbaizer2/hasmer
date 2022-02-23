using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmTokenStream {
        public AssemblerState State { get; set; }

        public HasmTokenStream(string hasm) {
            State = new AssemblerState {
                Stream = new HasmStringStream(hasm)
            };
        }

        public HasmTokenStream(AssemblerState state) {
            State = state;
        }

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
