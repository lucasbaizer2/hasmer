using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmVersionDeclarationToken : HasmToken {
        public HasmIntegerToken Version { get; set; }

        public HasmVersionDeclarationToken(HasmStringStreamState state) : base(state) { }
    }

    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum HasmDataDeclarationType {
        String,
        Integer,
        Number,
        Null,
        True,
        False
    }

    public class HasmDataDeclarationToken : HasmToken {
        public HasmLabelToken Label { get; set; }
        public HasmDataDeclarationType DataType { get; set; }
        public List<HasmLiteralToken> Data { get; set; }

        public HasmDataDeclarationToken(HasmStringStreamState state) : base(state) { }
    }

    public class HasmDeclarationParser : IHasmTokenParser {
        public bool CanParse(AssemblerState asm) {
            string op = asm.Stream.PeekOperator();
            if (op != ".") {
                return false;
            }
            asm.Stream.AdvanceOperator();

            string word = asm.Stream.PeekWord();
            if (word == null) {
                return false;
            }

            return word == "hasm" || word == "data" || word == "start" || word == "end" || word == "id" || word == "params" || word == "registers" || word == "symbols" || word == "label" || word == "strict";
        }

        public HasmToken Parse(AssemblerState asm) {
            HasmStringStreamState state = asm.Stream.SaveState();

            asm.Stream.AdvanceOperator(); // skip "." before declaration
            string word = asm.Stream.AdvanceWord();
            if (word == "hasm") {
                return new HasmVersionDeclarationToken(state) {
                    Version = (HasmIntegerToken)IHasmTokenParser.IntegerParser.Parse(asm)
                };
            } else if (word == "data") {
                HasmLabelToken label = (HasmLabelToken)IHasmTokenParser.LabelParser.Parse(asm);
                string dataTypeRaw = asm.Stream.PeekWord();
                if (dataTypeRaw == null) {
                    throw new HasmParserException(asm.Stream, "expecting data type");
                }
                asm.Stream.AdvanceWord();
                if (!Enum.IsDefined(typeof(HasmDataDeclarationType), dataTypeRaw)) {
                    throw new HasmParserException(asm.Stream, $"invalid data type: '{dataTypeRaw}'");
                }
                HasmDataDeclarationType dataType = Enum.Parse<HasmDataDeclarationType>(dataTypeRaw);
                if (asm.Stream.PeekOperator() != "[") {
                    throw new HasmParserException(asm.Stream, "expecting '['");
                }
                asm.Stream.AdvanceOperator();

                IHasmTokenParser dataParser = dataType switch {
                    HasmDataDeclarationType.String => IHasmTokenParser.StringParser,
                    HasmDataDeclarationType.Integer => IHasmTokenParser.IntegerParser,
                    HasmDataDeclarationType.Number => IHasmTokenParser.NumberParser,
                    HasmDataDeclarationType.Null => new HasmSimpleParser("null"),
                    HasmDataDeclarationType.True => new HasmSimpleParser("true"),
                    HasmDataDeclarationType.False => new HasmSimpleParser("false"),
                    _ => throw new NotImplementedException()
                };

                List<HasmLiteralToken> data = new List<HasmLiteralToken>();
                while (true) {
                    data.Add((HasmLiteralToken)dataParser.Parse(asm));
                    string peek = asm.Stream.PeekOperator();
                    if (peek == "]") {
                        asm.Stream.AdvanceOperator();
                        break;
                    }
                    if (peek != ",") {
                        throw new HasmParserException(asm.Stream, "expecting either ']' or ','");
                    }
                    asm.Stream.AdvanceOperator();
                }

                return new HasmDataDeclarationToken(state) {
                    Label = label,
                    DataType = dataType,
                    Data = data
                };
            } else if (word == "start") {
                string functionType = asm.Stream.PeekWord();
                if (functionType != "Function" && functionType != "Constructor" && functionType != "NCFunction") {
                    throw new HasmParserException(asm.Stream, "expecting either 'Function', 'Constructor', or 'NCFunction'");
                }
                asm.Stream.AdvanceWord();

                if (asm.Stream.PeekOperator() != "<") {
                    throw new HasmParserException(asm.Stream, "expecting function name in angled brackets");
                }
                asm.Stream.AdvanceOperator();

                asm.Stream.WhitespaceMode = HasmStringStreamWhitespaceMode.Keep;
                StringBuilder builder = new StringBuilder();
                char lastChar = '\0';
                while (asm.Stream.PeekCharacters(1) != null) {
                    string character = asm.Stream.AdvanceCharacters(1);
                    if (character == ">" && lastChar != '\\') {
                        break;
                    }
                    builder.Append(character);
                    lastChar = character[0];
                }
                asm.Stream.WhitespaceMode = HasmStringStreamWhitespaceMode.Remove;

                asm.Stream.CurrentLine++;
                asm.Stream.CurrentColumn = 0;

                List<HasmToken> body = new List<HasmToken>();
                HasmFunctionToken function = new HasmFunctionToken(state) {
                    FunctionName = builder.ToString(),
                    Body = body
                };
                asm.CurrentFunction = function;

                HasmTokenStream bodyStream = new HasmTokenStream(asm);
                foreach (HasmToken token in bodyStream.ReadTokens()) {
                    if (token is HasmSimpleToken simple && simple.Value == "end") {
                        asm.CurrentFunction = null;
                        break;
                    }
                    body.Add(token);
                }

                return function;
            } else if (word == "end") {
                return new HasmSimpleToken(state) {
                    Value = "end"
                };
            } else if (word == "id" || word == "params" || word == "registers" || word == "symbols" || word == "label" || word == "strict") {
                if (word != "strict") {
                    asm.Stream.AdvanceWord();
                }
                return null;
            }

            return null;
        }
    }
}
