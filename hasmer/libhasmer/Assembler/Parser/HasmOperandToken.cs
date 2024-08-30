using System;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a Hasm instruction's operand.
    /// </summary>
    public class HasmOperandToken : HasmToken {
        /// <summary>
        /// The type of the operand.
        /// </summary>
        public HasmOperandTokenType OperandType { get; set; }

        /// <summary>
        /// If the operand represents a string, then its kind is returned (literal or identifier).
        /// Otherwise, if this operand is something other than a string, an exception is thrown.
        /// </summary>
        public StringKind OperandStringKind => OperandType switch {
            HasmOperandTokenType.String => StringKind.Literal,
            HasmOperandTokenType.Identifier => StringKind.Identifier,
            _ => throw new Exception("operand is not a string")
        };

        /// <summary>
        /// The value represented by the operand.
        /// </summary>
        public PrimitiveValue Value { get; set; }

        public HasmOperandToken(HasmStringStreamState? state) : base(state) { }
    }
}
