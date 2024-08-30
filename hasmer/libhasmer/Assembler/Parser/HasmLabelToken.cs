namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents the type of a Hasm label.
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum HasmLabelKind {
        /// <summary>
        /// A reference or declaration in the array buffer.
        /// </summary>
        ArrayBuffer = 'A',
        /// <summary>
        /// A reference or declaration in the object key buffer.
        /// </summary>
        ObjectKeyBuffer = 'K',
        /// <summary>
        /// A reference or declaration in the object value buffer.
        /// </summary>
        ObjectValueBuffer = 'V',
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
        /// The type of the label, e.g. label = "L5", LabelType = 'L'
        /// </summary>
        public HasmLabelKind Kind { get; set; }

        /// <summary>
        /// The index of the label, e.g. label = "L5", LabelIndex = 5
        /// </summary>
        public HasmIntegerToken Index { get; set; }

        /// <summary>
        /// The offset after the label, used in label references. Example: label reference = "L5-6", Offset = -6
        /// </summary>
        public HasmIntegerToken ReferenceOffset { get; set; }

        public HasmLabelToken(HasmStringStreamState state) : base(state) { }
    }
}
