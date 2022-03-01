using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents a token in a Hasm assembly file.
    /// </summary>
    public class HasmToken {
        /// <summary>
        /// The line of the file that the token is on. This is zero-indexed, i.e. the first line (Line = 0), the second line (Line = 1), etc.
        /// </summary>
        [JsonIgnore]
        public int Line { get; set; }
        /// <summary>
        /// The column of the line of the file that the token is on. This is zero-indexed, i.e. the first column (Column = 0), the second column (Column = 1), etc.
        /// </summary>
        [JsonIgnore]
        public int Column { get; set; }

#pragma warning disable IDE0051 // Remove unused private members
        /// <summary>
        /// Gets the type of the token (the class name of the implementation) as information when JSON serializing the token (used for debug).
        /// </summary>
        [JsonProperty("TokenType")]
        private string JsonTokenType => GetType().Name;

        /// <summary>
        /// Gets the line of the token (except 1-indexed) as information when JSON serializing the token (used for debug).
        /// </summary>
        [JsonProperty("Line")]
        private int JsonLine => Line + 1;

        /// <summary>
        /// Gets the column of the token (except 1-indexed) as information when JSON serializing the token (used for debug).
        /// </summary>
        [JsonProperty("Column")]
        private int JsonColumn => Column + 1;
#pragma warning restore IDE0051 // Remove unused private members

        /// <summary>
        /// Creates a new HasmToken from the given state. This does not parse anything.
        /// </summary>
        public HasmToken(HasmStringStreamState state) {
            Line = state.CurrentLine;
            Column = state.CurrentColumn;
        }

        /// <summary>
        /// Serializes the token to JSON. 
        /// </summary>
        public void Write(SourceCodeBuilder builder) {
            string serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
            builder.Write(serialized);
        }
    }
}
