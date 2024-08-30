namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// The type of the operand.
    /// </summary>
    public enum HasmOperandTokenType {
        /// <summary>
        /// The operand is a register reference.
        /// </summary>
        Register,

        /// <summary>
        /// The operand is a code label reference.
        /// </summary>
        Label,

        /// <summary>
        /// The operand is an unsigned integer.
        /// </summary>
        UInt,

        /// <summary>
        /// The operand is a string literal.
        /// </summary>
        String,

        /// <summary>
        /// The operand is an identifier.
        /// </summary>
        Identifier,

        /// <summary>
        /// The operand is an eight-byte IEEE754 float-point number.
        /// </summary>
        Double
    }
}
