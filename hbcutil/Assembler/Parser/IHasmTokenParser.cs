using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public interface IHasmTokenParser {
        public static readonly IHasmTokenParser CommentParser = new HasmCommentParser();
        public static readonly IHasmTokenParser DeclarationParser = new HasmDeclarationParser();
        public static readonly IHasmTokenParser IntegerParser = new HasmIntegerParser();
        public static readonly IHasmTokenParser NumberParser = new HasmNumberParser();
        public static readonly IHasmTokenParser StringParser = new HasmStringParser();
        public static readonly IHasmTokenParser LabelParser = new HasmLabelParser();
        public static readonly IHasmTokenParser InstructionParser = new HasmInstructionParser();

        bool CanParse(AssemblerState asm);

        HasmToken Parse(AssemblerState asm);
    }
}
