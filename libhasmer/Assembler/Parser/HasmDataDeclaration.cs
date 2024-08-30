using System.Collections.Generic;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// The kind of data being declared in a ".data" declaration.
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum HasmDataDeclarationKind {
        String,
        Integer,
        Number,
        Null,
        True,
        False
    }

    /// <summary>
    /// Represents a ".data" declaration.
    /// </summary>
    public class HasmDataDeclaration {
        /// <summary>
        /// The label of the data.
        /// </summary>
        public HasmLabelToken Label { get; set; }

        /// <summary>
        /// The kind of the data.
        /// </summary>
        public HasmDataDeclarationKind Kind { get; set; }

        /// <summary>
        /// The amount of distinct items in the data declaration.
        /// In declarations where there are specific elements, this is equal to <see cref="Elements.Length" />.
        /// In declarations where elements are repeated (i.e. Null, True, or False), this is equal to the supplied repeat count.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The tokens that define the contents of the data, if there are specific elements.
        /// </summary>
        public List<HasmLiteralToken> Elements { get; set; }
    }
}
