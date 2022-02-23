using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum LabelType {
        ArrayBuffer = 'A',
        KeyBuffer = 'K',
        ValueBuffer = 'V',
        CodeLabel = 'L'
    }

    public class HasmLabelToken : HasmToken {
        public LabelType LabelType { get; set; }
        public HasmIntegerToken LabelIndex { get; set; }
        public int? DeclaredOffset { get; set; }

        public HasmLabelToken(HasmStringStreamState state) : base(state) { }
    }

    public class HasmLabelParser : IHasmTokenParser {
        public bool CanParse(AssemblerState asm) {
            string labelType = asm.Stream.PeekCharacters(1);
            if (labelType == null) {
                return false;
            }

            if (!Enum.IsDefined(typeof(LabelType), (int)labelType[0])) {
                return false;
            }

            HasmStringStreamState state = asm.Stream.SaveState();
            asm.Stream.AdvanceCharacters(1); // skip label type
            if (!IHasmTokenParser.IntegerParser.CanParse(asm)) {
                asm.Stream.LoadState(state);
                return false;
            }

            if (asm.Stream.PeekOperator() == "+" || asm.Stream.PeekOperator() == "-") {
                asm.Stream.AdvanceOperator();
                if (!IHasmTokenParser.IntegerParser.CanParse(asm)) {
                    asm.Stream.LoadState(state);
                    return false;
                }
            }

            asm.Stream.LoadState(state);
            return true;
        }

        public HasmToken Parse(AssemblerState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, "invalid label");
            }

            HasmStringStreamState state = asm.Stream.SaveState();
            LabelType labelType = (LabelType)asm.Stream.AdvanceCharacters(1)[0];
            HasmIntegerToken labelIndex = (HasmIntegerToken)IHasmTokenParser.IntegerParser.Parse(asm);
            int? declaredOffset = null;
            if (asm.Stream.PeekOperator() == "+" || asm.Stream.PeekOperator() == "-") {
                string op = asm.Stream.AdvanceOperator();
                HasmIntegerToken labelOffset = (HasmIntegerToken)IHasmTokenParser.IntegerParser.Parse(asm);
                int offset = labelOffset.GetValueAsInt32();
                if (op == "-") {
                    offset = -offset;
                }
                declaredOffset = offset;
            }
            return new HasmLabelToken(state) {
                LabelType = labelType,
                LabelIndex = labelIndex,
                DeclaredOffset = declaredOffset
            };
        }
    }
}
