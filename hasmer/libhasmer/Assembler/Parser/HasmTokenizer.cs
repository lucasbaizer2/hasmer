using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hasmer.Assembler.Parser {
    enum StringParserState {
        Normal,
        Escape
    }

    public class TokenDefinition {
        [JsonProperty]
        public string Kind { get; set; }

        [JsonProperty]
        public dynamic Match { get; set; }

        [JsonProperty]
        public string Var { get; set; }

        [JsonProperty]
        public string Modifier { get; set; }

        [JsonProperty]
        public bool Repeated { get; set; }

        [JsonProperty]
        public bool Optional { get; set; }
    }

    public class TokenizerState {
        public HasmStringStream Stream { get; set; }

        public Dictionary<string, List<TokenMatch>> Vars { get; set; }
        public bool Moved { get; set; }
        public HasmStringStreamWhitespaceMode WhitespaceMode { get; set; }

        public void AddVar(string name, TokenMatch token) {
            if (Vars.TryGetValue(name, out List<TokenMatch> l)) {
                l.Add(token);
            } else {
                l = new List<TokenMatch>(1);
                l.Add(token);
                Vars[name] = l;
            }
        }

        public TokenizerState Derive() {
            return new TokenizerState {
                WhitespaceMode = HasmStringStreamWhitespaceMode.Remove,
                Stream = Stream,
                Vars = new Dictionary<string, List<TokenMatch>>(),
                Moved = false,
            };
        }
    }

    public class TokenMatch {
        public object Value;

        public TokenMatch(Dictionary<string, List<TokenMatch>> matches) {
            Value = matches;
        }

        public TokenMatch(HasmToken token) {
            Value = token;
        }

        public Dictionary<string, List<TokenMatch>> AsToken() {
            return (Dictionary<string, List<TokenMatch>>) Value;
        }

        public HasmIntegerToken AsInteger() {
            return (HasmIntegerToken) Value;
        }

        public HasmStringToken AsString() {
            return (HasmStringToken) Value;
        }

        public HasmSimpleToken AsSimple() {
            return (HasmSimpleToken) Value;
        }
    }

    public enum TokenizerResultKind {
        TokenMatch,
        EmptyMatch,
        FailedMatch
    }

    public struct TokenizerResult {
        public TokenizerResultKind Kind { get; set; }
        public TokenMatch Token { get; set; }

        public static TokenizerResult Matched(HasmToken token) {
            return new TokenizerResult {
                Kind = TokenizerResultKind.TokenMatch,
                Token = new TokenMatch(token),
            };
        }

        public static TokenizerResult Empty {
            get {
                return new TokenizerResult {
                    Kind = TokenizerResultKind.EmptyMatch
                };
            }
        }

        public static TokenizerResult Failed {
            get {
                return new TokenizerResult {
                    Kind = TokenizerResultKind.FailedMatch
                };
            }
        }
    }

    public delegate TokenizerResult Tokenizer(TokenizerState state);

    public class HasmTokenizer {
        private List<string> ParseStack = new List<string>();
        private Dictionary<string, Tokenizer> Tokenizers = new Dictionary<string, Tokenizer>();

        private string ParseStackText => string.Join(" -> ", ParseStack);

        public HasmTokenizer() : this(ResourceManager.ReadEmbeddedResource<Dictionary<string, IEnumerable<TokenDefinition>>>("HasmTokenDefinitions")) { }

        public HasmTokenizer(Dictionary<string, IEnumerable<TokenDefinition>> defsMap) {
            foreach (KeyValuePair<string, IEnumerable<TokenDefinition>> pair in defsMap) {
                string name = pair.Key;
                IEnumerable<TokenDefinition> defs = pair.Value;

                Tokenizers[name] = CreateMultiTokenParser(defs.Select(CreateFullParser).ToList());
            }
        }

        private HasmHeader TokenizeHeader(TokenMatch match) {
            Dictionary<string, List<TokenMatch>> vars = match.AsToken();
            HasmIntegerToken version = vars["version"][0].AsInteger();
            HasmSimpleToken mode = vars["mode"][0].AsSimple();

            bool isExact;
            if (mode.Value == "auto") {
                isExact = false;
            } else if (mode.Value == "exact") {
                isExact = true;
            } else {
                throw new HasmParserException(mode, "invalid mode; expecting either 'auto' or 'exact'");
            }

            return new HasmHeader {
                Version = version,
                IsExact = isExact,
            };
        }

        private HasmLabelToken TokenizeLabel(TokenMatch match) {
            Dictionary<string, List<TokenMatch>> vars = match.AsToken();
            HasmSimpleToken kind = vars["kind"][0].AsSimple();
            HasmLabelKind label = kind.Value switch {
                "A" => HasmLabelKind.ArrayBuffer,
                "K" => HasmLabelKind.ObjectKeyBuffer,
                "V" => HasmLabelKind.ObjectValueBuffer,
                "L" => HasmLabelKind.CodeLabel,
                _ => throw new HasmParserException(kind, "invalid label type; expecting 'A', 'K', 'V', or 'L'")
            };
            HasmIntegerToken index = vars["index"][0].AsInteger();
            HasmIntegerToken offset = null;
            if (vars.ContainsKey("offset")) {
                offset = vars["offset"][0].AsInteger();
            }

            return new HasmLabelToken(kind.AsStreamState()) {
                Kind = label,
                Index = index,
                ReferenceOffset = offset,
            };
        }

        private HasmLiteralToken TokenizeLiteral(TokenMatch match) {
            if (match.Value is Dictionary<string, List<TokenMatch>> vars) {
                Console.WriteLine($"vars: {vars.Count}");
                foreach (KeyValuePair<string, List<TokenMatch>> entry in vars) {
                    Console.WriteLine($"  {entry.Key} = {entry.Value}");
                }
            }
            if (match.Value is HasmLiteralToken lit) {
                return lit;
            }
            throw new Exception($"invalid literal match value type: {match.Value.GetType().FullName}");
        }

        private HasmOperandToken TokenizeOperand(TokenMatch match) {
            // if (match.Value is HasmStringToken str) {
            //     return new HasmOperandToken(str.AsStreamState()) {
            //         OperandType = HasmOperandTokenType.String,
            //         Value = new PrimitiveType(str.Value),
            //     };
            // }
            // if (match.Value is HasmIdentifierToken ident) {
            //     return new HasmOperandToken(ident.AsStreamState()) {
            //         OperandType = HasmOperandTokenType.Identifier,
            //         Value = new PrimitiveType(ident.Value),
            //     };
            // }
            // if (match.Value is HasmIntegerToken integer) {
            //     return new HasmOperandToken(ident.AsStreamState()) {
            //         OperandType = HasmOperandTokenType.UInt,
            //         Value = new PrimitiveType(integer.Value),
            //     };
            // }
            // if (match.Value is Dictionary<string, List<TokenMatch>> vars) {
            //     if (vars.ContainsKey("reg")) {

            //     } else {
            //         throw new Exception("invalid operand");
            //     }
            // }
            throw new Exception($"invalid operand match value type: {match.Value.GetType().FullName}");
        }

        private HasmDataDeclaration TokenizeDataDeclaration(TokenMatch match) {
            Dictionary<string, List<TokenMatch>> vars = match.AsToken();

            HasmSimpleToken declToken = vars["decl"][0].AsSimple();
            HasmLabelToken label = TokenizeLabel(vars["label"][0]);
            HasmSimpleToken kindToken = vars["kind"][0].AsSimple();

            if (!vars.ContainsKey("length") && !vars.ContainsKey("elements")) {
                throw new HasmParserException(declToken, $"expecting either data length or specified elements");
            }

            HasmDataDeclarationKind kind = kindToken.Value switch {
                "String" => HasmDataDeclarationKind.String,
                "Integer" => HasmDataDeclarationKind.Integer,
                "Number" => HasmDataDeclarationKind.Number,
                "Null" => HasmDataDeclarationKind.Null,
                "True" => HasmDataDeclarationKind.True,
                "False" => HasmDataDeclarationKind.False,
                _ => throw new HasmParserException(kindToken, "invalid data kind")
            };

            bool isConstType = kind switch {
                HasmDataDeclarationKind.Null or HasmDataDeclarationKind.True or HasmDataDeclarationKind.False => true,
                _ => false
            };
            if (isConstType && vars.ContainsKey("elements")) {
                throw new HasmParserException(declToken, $"data declaration of kind {kindToken.Value} cannot have specified elements");
            }
            if (!isConstType && vars.ContainsKey("length")) {
                throw new HasmParserException(declToken, $"data declaration of kind {kindToken.Value} must have specified elements");
            }

            if (isConstType) {
                HasmIntegerToken length = vars["length"][0].AsInteger();

                return new HasmDataDeclaration {
                    Label = label,
                    Kind = kind,
                    Count = length.GetValueAsInt32(),
                };
            } else {
                List<HasmLiteralToken> elements = vars["elements"].Select(TokenizeLiteral).ToList();

                return new HasmDataDeclaration {
                    Label = label,
                    Kind = kind,
                    Count = elements.Count,
                    Elements = elements,
                };
            }
        }

        public HasmProgram TokenizeProgram(string input) {
            TokenizerState state = new TokenizerState {
                Stream = new HasmStringStream(input),
                Vars = new Dictionary<string, List<TokenMatch>>(),
                Moved = false,
                WhitespaceMode = HasmStringStreamWhitespaceMode.Remove,
            };

            ParseStack.Add("Program");
            TokenizerResult res = Tokenizers["Program"](state);
            ParseStack.RemoveAt(0);
            if (res.Kind == TokenizerResultKind.FailedMatch) {
                throw new Exception($"failed @ {state.Stream.CurrentLine},{state.Stream.CurrentColumn}");
            }
            HasmHeader header = TokenizeHeader(state.Vars["header"][0]);
            List<HasmDataDeclaration> data = state.Vars["data"].Select(TokenizeDataDeclaration).ToList();

            return new HasmProgram {
                Header = header,
                Data = data,
            };
        }

        private Tokenizer CreateMultiTokenParser(List<Tokenizer> parsers) {
            return state => {
                TokenizerResult res;
                HasmStringStreamState sss = state.Stream.SaveState();
                foreach (Tokenizer parser in parsers) {
                    state.Stream.WhitespaceMode = state.WhitespaceMode;
                    Console.WriteLine($"{ParseStackText}: MultiToken parser moving to next");
                    res = parser(state);
                    if (res.Kind == TokenizerResultKind.FailedMatch) {
                        Console.WriteLine($"{ParseStackText}: MultiTokenParser failed match (moved = {state.Moved})");
                        if (state.Moved) {
                            throw new HasmParserException(sss, "fatal parser error");
                        }

                        state.Stream.LoadState(sss);
                        return TokenizerResult.Failed;
                    }
                }

                return TokenizerResult.Empty;
            };
        }

        private Tokenizer CreateFullParser(TokenDefinition def) {
            Tokenizer inner = CreateCapturingParser(def);
            if (def.Optional && def.Repeated) {
                return state => {
                    while (true) {
                        HasmStringStreamState sss = state.Stream.SaveState();

                        Console.WriteLine($"{ParseStackText}: OptionalRepeated FullParser reading next");
                        TokenizerResult res = inner(state);
                        Console.WriteLine($"{ParseStackText}: OptionalRepeated FullParser read {res.Kind}");
                        if (res.Kind == TokenizerResultKind.FailedMatch) {
                            state.Stream.LoadState(sss);
                            break;
                        }
                    }
                    return TokenizerResult.Empty;
                };
            } else if (def.Optional) {
                return state => {
                    HasmStringStreamState sss = state.Stream.SaveState();

                    TokenizerResult res = inner(state);
                    if (res.Kind == TokenizerResultKind.FailedMatch) {
                        Console.WriteLine(ParseStackText + ": Optional FullParser failed to match (skipping)");
                        state.Stream.LoadState(sss);
                        return TokenizerResult.Empty;
                    }
                    return res;
                };
            } else if (def.Repeated) {
                return state => {                
                    TokenizerResult res = inner(state);
                    if (res.Kind == TokenizerResultKind.FailedMatch) {
                        Console.WriteLine(ParseStackText + ": Repeated FullParser failed to match initial");
                        return TokenizerResult.Failed;
                    }
                    while (true) {
                        HasmStringStreamState sss = state.Stream.SaveState();

                        res = inner(state);
                        if (res.Kind == TokenizerResultKind.FailedMatch) {
                            Console.WriteLine(ParseStackText + ": Repeated FullParser stopped matching");
                            state.Stream.LoadState(sss);
                            break;
                        }
                    }
                    return TokenizerResult.Empty;
                };
            } else {
                return inner;
            }
        }

        int offset = 0;

        private Tokenizer CreateCapturingParser(TokenDefinition def) {
            Tokenizer inner = CreateSimpleParser(def);
            return state => {
                TokenizerResult res = inner(state);
                if (res.Kind == TokenizerResultKind.TokenMatch && !string.IsNullOrEmpty(def.Var)) {
                    state.AddVar(def.Var, res.Token);
                }
                return res;
            };
        }

        private Tokenizer CreateSimpleParser(TokenDefinition def) {
            switch (def.Kind) {
                case "literal":
                    string match = (string)def.Match;
                    return state => {
                        HasmStringStreamState sss = state.Stream.SaveState();

                        string take = state.Stream.AdvanceCharacters(match.Length);
                        if (take == null) {
                            return TokenizerResult.Failed;
                        }
                        if (match == take) {
                            return TokenizerResult.Matched(new HasmSimpleToken(sss) {
                                Value = take
                            });
                        } else {
                            return TokenizerResult.Failed;
                        }
                    };
                case "string":
                    return state => {
                        HasmToken token = ParseStringLike(state);
                        if (token is HasmStringToken) {
                            return TokenizerResult.Matched(token);
                        } else {
                            return TokenizerResult.Failed;
                        }
                    };
                case "ident":
                    return state => {
                        HasmToken token = ParseStringLike(state);
                        if (token is HasmIdentifierToken) {
                            return TokenizerResult.Matched(token);
                        } else {
                            return TokenizerResult.Failed;
                        }
                    };
                case "word":
                    return state => {
                        HasmStringStreamState sss = state.Stream.SaveState();

                        string take = state.Stream.AdvanceWord();
                        if (take == null) {
                            return TokenizerResult.Failed;
                        }
                        return TokenizerResult.Matched(new HasmSimpleToken(sss) {
                            Value = take
                        });
                    };
                case "integer":
                    return state => {
                        HasmStringStreamState sss = state.Stream.SaveState();

                        long multiplier = 1;
                        char op = state.Stream.PeekOperator();
                        if (op == '-') { // negative number
                            state.Stream.AdvanceOperator();
                            multiplier = -1;
                        } else if (op == '+') {
                            state.Stream.AdvanceOperator();
                        } else if (op != '\0') {
                            return TokenizerResult.Failed;
                        }

                        string word = state.Stream.AdvanceWord();
                        if (word == null) {
                            return TokenizerResult.Failed;
                        }
                        long parsed = long.Parse(word);
                        return TokenizerResult.Matched(new HasmIntegerToken(sss, parsed * multiplier));
                    };
                case "number":
                    return state => {
                        HasmStringStreamState sss = state.Stream.SaveState();

                        double multiplier = 1.0;
                        char op = state.Stream.PeekOperator();
                        if (op == '-') { // negative number
                            state.Stream.AdvanceOperator();
                            multiplier = -1.0;
                        } else if (op == '+') {
                            state.Stream.AdvanceOperator();
                        } else if (op != '\0') {
                            return TokenizerResult.Failed;
                        }

                        string intPart = state.Stream.AdvanceWord();
                        if (intPart == null) {
                            return TokenizerResult.Failed;
                        }
                        char separator = state.Stream.PeekOperator();
                        HasmNumberToken token;
                        if (separator == '.') { // separator == ".", it's a fraction
                            state.Stream.AdvanceOperator();
                            string fractionPart = state.Stream.AdvanceWord();
                            if (fractionPart == null) {
                                return TokenizerResult.Failed;
                            }
                            token = new HasmNumberToken(sss) {
                                Value = double.Parse($"{intPart}.{fractionPart}") * multiplier
                            };
                        } else {
                            if (intPart == "Infinity") {
                                token = new HasmNumberToken(sss) {
                                    Value = multiplier == 1.0 ? double.PositiveInfinity : double.NegativeInfinity
                                };
                            } else if (intPart == "NaN") {
                                token = new HasmNumberToken(sss) {
                                    Value = double.NaN
                                };
                            } else {
                                token = new HasmNumberToken(sss) {
                                    Value = double.Parse(intPart) * multiplier
                                };
                            }
                        }

                        return TokenizerResult.Matched(token);
                    };
                case "enum":
                    JArray matchArr = (JArray)def.Match;
                    IEnumerable<string> values = matchArr.Values<string>();
                    return state => {
                        HasmStringStreamState sss = state.Stream.SaveState();
                        foreach (string value in values) {
                            state.Stream.LoadState(sss);

                            string take = state.Stream.AdvanceCharacters(value.Length);
                            if (take == null) {
                                continue;
                            }
                            if (value == take) {
                                return TokenizerResult.Matched(new HasmSimpleToken(sss) {
                                    Value = value
                                });
                            }
                        }

                        state.Stream.LoadState(sss);
                        return TokenizerResult.Failed;
                    };
                case "or":
                    JArray matches = (JArray)def.Match;
                    IEnumerable<Tokenizer> tokenizers = matches.ToObject<List<TokenDefinition>>().Select(CreateFullParser);
                    return state => {
                        HasmStringStreamState sss = state.Stream.SaveState();
                        foreach (Tokenizer tokenizer in tokenizers) {
                            state.Stream.LoadState(sss);
                            TokenizerResult res = tokenizer(state);
                            if (res.Kind != TokenizerResultKind.FailedMatch) {
                                return res;
                            }
                        }

                        state.Stream.LoadState(sss);
                        return TokenizerResult.Failed;
                    };
                default:
                    if (def.Modifier != null) {
                        switch (def.Modifier) {
                            case "contiguous":
                                return state => {
                                    state.WhitespaceMode = HasmStringStreamWhitespaceMode.Keep;
                                    return TokenizerResult.Empty;
                                };
                            case "move":
                                return state => {
                                    state.Moved = true;
                                    return TokenizerResult.Empty;
                                };
                            default:
                                throw new Exception($"invalid modifier: {def.Modifier}");
                        }
                    }

                    if (def.Match is JArray matchArray) {
                        List<TokenDefinition> tokens = matchArray.ToObject<List<TokenDefinition>>();
                        return CreateMultiTokenParser(tokens.Select(CreateFullParser).ToList());
                    }

                    if (def.Kind[0] == '$') {
                        Tokenizer tokenizer = Tokenizers[def.Kind[1..]];
                        return state => {
                            ParseStack.Add(def.Kind[1..]);
                            TokenizerState derived = state.Derive();
                            TokenizerResult res = tokenizer(derived);
                            if (res.Kind == TokenizerResultKind.FailedMatch) {
                                ParseStack.RemoveAt(ParseStack.Count - 1);
                                return res;
                            }
                            if (!string.IsNullOrEmpty(def.Var)) {
                                if (derived.Vars.Count > 0) {
                                    state.AddVar(def.Var, new TokenMatch(derived.Vars));
                                } else if (res.Token != null) {
                                    state.AddVar(def.Var, res.Token);
                                }
                            }
                            ParseStack.RemoveAt(ParseStack.Count - 1);
                            return TokenizerResult.Empty;
                        };
                    } else {
                        throw new Exception($"invalid token kind: {def.Kind}");
                    }
            }
        }

        private static HasmLiteralToken ParseStringLike(TokenizerState state) {
            HasmStringStreamWhitespaceMode prevMode = state.Stream.WhitespaceMode;
            state.Stream.WhitespaceMode = HasmStringStreamWhitespaceMode.Keep;

            char c = state.Stream.PeekChar();
            if (c == '\0' || c != '"' && c != '<') {
                return null;
            }
            HasmStringStreamState streamState = state.Stream.SaveState();
            state.Stream.Advance(1);

            StringBuilder res = new StringBuilder();
            StringParserState parserState = StringParserState.Normal;
            if (c == '"') {
                while ((c = state.Stream.AdvanceChar()) != '\0') {
                    if (parserState == StringParserState.Normal) {
                        if (c == '"') {
                            state.Stream.WhitespaceMode = prevMode;
                            return new HasmStringToken(streamState) {
                                Value = res.ToString(),
                            };
                        } else if (c == '\\') {
                            parserState = StringParserState.Escape;
                        } else {
                            res.Append(c);
                        }
                    } else if (parserState == StringParserState.Escape) {
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
                                string hex = state.Stream.AdvanceCharacters(4);
                                if (hex == null) {
                                    state.Stream.WhitespaceMode = prevMode;
                                    return null;
                                }
                                m = (char)Convert.ToInt32(hex, 16);
                                break;
                            default:
                                throw new Exception($"invalid string escape code: \\{c}");
                        }

                        parserState = StringParserState.Normal;
                        res.Append(m);
                    }
                }
            } else if (c == '<') {
                while ((c = state.Stream.AdvanceChar()) != '\0') {
                    if (parserState == StringParserState.Normal) {
                        if (c == '>') {
                            state.Stream.WhitespaceMode = prevMode;
                            return new HasmIdentifierToken(streamState) {
                                Value = res.ToString(),
                            };
                        } else if (c == '\\') {
                            parserState = StringParserState.Escape;
                        } else {
                            res.Append(c);
                        }
                    } else if (parserState == StringParserState.Escape) {
                        char m;
                        switch (c) {
                            case '<': m = '<'; break;
                            case '>': m = '>'; break;
                            case '\\': m = '\\'; break;
                            default:
                                throw new Exception($"invalid identifier escape code: \\{c}");
                        }

                        parserState = StringParserState.Normal;
                        res.Append(m);
                    }
                }
            }

            state.Stream.WhitespaceMode = prevMode;
            return null;
        }
    }
}
