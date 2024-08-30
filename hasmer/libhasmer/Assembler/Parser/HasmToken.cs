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
        /// The offset in characters from the start of the file.
        /// </summary>
        [JsonIgnore]
        public int Offset { get; set; }

        public int Line { get; set; }
        public int Column { get; set; }

        /// <summary>
        /// Creates a new HasmToken from the given state. This does not parse anything.
        /// </summary>
        public HasmToken(HasmStringStreamState? state) {
            if (state == null) {
                return;
            }
            HasmStringStreamState s = state.Value;
            Offset = s.Offset;
            Line = s.Line;
            Column = s.Column;
        }

        public HasmStringStreamState AsStreamState() {
            return new HasmStringStreamState{
                Offset = Offset,
                Line = Line,
                Column = Column,
            };
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
