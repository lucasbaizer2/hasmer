using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    /// <summary>
    /// Represents an integer, either a 4-byte signed integer or a 4-byte unsigned integer specifically.
    /// </summary>
    public class HasmIntegerToken : HasmLiteralToken {
        /// <summary>
        /// The integer value. This is stored a long to be able to handle both int and uint types.
        /// </summary>
        private long Value { get; set; }

        /// <summary>
        /// Returns the value as a 4-byte unsigned integer; throws an exception if the value is not in bounds.
        /// </summary>
        public uint GetValueAsUInt32() {
            if (Value < uint.MinValue || Value > uint.MaxValue) {
                throw new HasmParserException(Line, Column, $"integer is not uint: {Value}");
            }
            return (uint)Value;
        }

        /// <summary>
        /// Returns the value as a 4-byte signed integer; throws an exception if the value is not in bounds.
        /// </summary>
        public int GetValueAsInt32() {
            if (Value < int.MinValue || Value > int.MaxValue) {
                throw new HasmParserException(Line, Column, $"integer is not int: {Value}");
            }
            return (int)Value;
        }

        public HasmIntegerToken(HasmStringStreamState state, long value) : base(state) {
            Value = value;
        }
    }

    /// <summary>
    /// Parses an integer.
    /// </summary>
    public class HasmIntegerParser : IHasmTokenParser {
        public bool CanParse(HasmReaderState asm) {
            HasmStringStreamState state = asm.Stream.SaveState();
            if (asm.Stream.PeekOperator() == "-") { // negative number
                asm.Stream.AdvanceOperator();
            }

            string word = asm.Stream.PeekWord();
            if (word == null) {
                asm.Stream.LoadState(state);
                return false;
            }

            asm.Stream.LoadState(state);
            return long.TryParse(word, out long _);
        }

        public HasmToken Parse(HasmReaderState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, "invalid integer");
            }

            HasmStringStreamState state = asm.Stream.SaveState();

            int multiplier = 1;
            if (asm.Stream.PeekOperator() == "-") { // negative number
                asm.Stream.AdvanceOperator();
                multiplier = -1;
            }

            string word = asm.Stream.AdvanceWord();
            return new HasmIntegerToken(state, long.Parse(word) * multiplier);
        }
    }
}
