using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HbcUtil.Assembler.Parser {
    public class HasmToken {
        [JsonIgnore]
        public int Line { get; set; }
        [JsonIgnore]
        public int Column { get; set; }

#pragma warning disable IDE0051 // Remove unused private members
        [JsonProperty("TokenType")]
        private string JsonTokenType => GetType().Name;
        [JsonProperty("Line")]
        private int JsonLine => Line + 1;
        [JsonProperty("Column")]
        private int JsonColumn => Column + 1;
#pragma warning restore IDE0051 // Remove unused private members

        public HasmToken(HasmStringStreamState state) {
            Line = state.CurrentLine;
            Column = state.CurrentColumn;
        }

        public void Write(SourceCodeBuilder builder) {
            string serialized = JsonConvert.SerializeObject(this, Formatting.Indented);
            builder.Write(serialized);
        }
    }
}
