using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    /// <summary>
    /// Represents a "simple" value, generally a constant identifier (e.g. "true" or "false").
    /// </summary>
    public class HasmSimpleToken : HasmLiteralToken {
        /// <summary>
        /// The simple value, defined by <see cref="HasmSimpleParser.Target"/>.
        /// </summary>
        public string Value { get; set; }

        public HasmSimpleToken(HasmStringStreamState state) : base(state) { }
    }

    /// <summary>
    /// Parses a simple value.
    /// </summary>
    public class HasmSimpleParser : IHasmTokenParser {
        /// <summary>
        /// The simple value to parse. The parser will only succeed if the stream is pointing at the target token.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Creates a new HasmSimpleParser that will only parse the given target.
        /// </summary>
        public HasmSimpleParser(string target) {
            Target = target;
        }

        public bool CanParse(HasmReaderState asm) {
            return asm.Stream.PeekWord() == Target;
        }

        public HasmToken Parse(HasmReaderState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, $"invalid simple; expecting '{Target}'");
            }

            HasmStringStreamState state = asm.Stream.SaveState();
            return new HasmSimpleToken(state) {
                Value = asm.Stream.AdvanceWord()
            };
        }
    }
}
