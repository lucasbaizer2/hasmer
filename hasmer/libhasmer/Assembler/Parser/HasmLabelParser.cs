using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents the type of a Hasm label.
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum LabelType {
        /// <summary>
        /// A reference or declaration in the array buffer.
        /// </summary>
        ArrayBuffer = 'A',
        /// <summary>
        /// A reference or declaration in the object key buffer.
        /// </summary>
        KeyBuffer = 'K',
        /// <summary>
        /// A reference or declaration in the object value buffer.
        /// </summary>
        ValueBuffer = 'V',
        /// <summary>
        /// A reference or declaration of an offset of code in a function's body, used for jump instructions.
        /// </summary>
        CodeLabel = 'L'
    }

    /// <summary>
    /// Represents a label definition or reference.
    /// </summary>
    public class HasmLabelToken : HasmToken {
        /// <summary>
        /// The type of the label, i.e. label = "L5", LabelType = 'L'
        /// </summary>
        public LabelType LabelType { get; set; }
        /// <summary>
        /// The index of the label, i.e. label = "L5", LabelIndex = 5
        /// </summary>
        public HasmIntegerToken LabelIndex { get; set; }
        /// <summary>
        /// The offset after the label, used in label references. Example: label reference = "L5-6", DeclaredOffset = -6
        /// </summary>
        public int? DeclaredOffset { get; set; }

        public HasmLabelToken(HasmStringStreamState state) : base(state) { }
    }

    /// <summary>
    /// Parses a label definition or reference.
    /// </summary>
    public class HasmLabelParser : IHasmTokenParser {
        public bool CanParse(HasmReaderState asm) {
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

        public HasmToken Parse(HasmReaderState asm) {
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
