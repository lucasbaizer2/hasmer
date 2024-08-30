
namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// The type of declaration for a function modifier.
    /// </summary>
    public enum HasmFunctionModifierType {
        Id,
        Params,
        Registers,
        Symbols,
        Strict
    }

    /// <summary>
    /// Represents a declaration that modifiers a function header.
    /// </summary>
    public class HasmFunctionModifierToken : HasmToken {
        /// <summary>
        /// The type of modifier.
        /// </summary>
        public HasmFunctionModifierType ModifierType { get; set; }

        /// <summary>
        /// The value associated with the modifier, or null if the modifier does not have a value.
        /// </summary>
        public uint? Value { get; set; }


        public HasmFunctionModifierToken(HasmStringStreamState state) : base(state) { }
    }
}
