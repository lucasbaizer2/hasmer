using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    /// <summary>
    /// Represents an 8-byte IEEE754 floating-point value.
    /// 
    /// This is directly equivalent to the "number" type in JavaScript.
    /// </summary>
    public class HasmNumberToken : HasmLiteralToken {
        /// <summary>
        /// The parsed value.
        /// </summary>
        public double Value { get; set; }

        public HasmNumberToken(HasmStringStreamState state) : base(state) { }
    }

    /// <summary>
    /// Parses an 8-byte IEEE754 floating-point value.
    /// </summary>
    public class HasmNumberParser : IHasmTokenParser {
        public bool CanParse(HasmReaderState asm) {
            HasmStringStreamState state = asm.Stream.SaveState();

            if (asm.Stream.PeekOperator() == "-") { // negative number
                asm.Stream.AdvanceOperator();
            }

            string intPart = asm.Stream.PeekWord();
            if (intPart == null) {
                return false;
            }

            asm.Stream.AdvanceWord(); // skip int part
            string separator = asm.Stream.PeekOperator();
            if (separator == ".") {
                asm.Stream.AdvanceOperator();
                string fractionPart = asm.Stream.PeekWord();
                if (fractionPart == null) {
                    return false;
                }
                asm.Stream.LoadState(state);
                return double.TryParse($"{intPart}.{fractionPart}", out double _);
            }
            asm.Stream.LoadState(state);
            if (intPart == "Infinity" || intPart == "NaN") {
                return true;
            }
            return double.TryParse(intPart, out double _);
        }

        public HasmToken Parse(HasmReaderState asm) {
            if (!CanParse(asm)) {
                throw new HasmParserException(asm.Stream, "invalid number");
            }

            HasmStringStreamState state = asm.Stream.SaveState();

            double multiplier = 1.0;
            if (asm.Stream.PeekOperator() == "-") { // negative number
                asm.Stream.AdvanceOperator();
                multiplier = -1.0;
            }

            string intPart = asm.Stream.AdvanceWord();
            string separator = asm.Stream.PeekOperator();
            if (separator == ".") { // separator == ".", it's a fraction
                asm.Stream.AdvanceOperator();
                string fractionPart = asm.Stream.AdvanceWord();
                return new HasmNumberToken(state) {
                    Value = double.Parse($"{intPart}.{fractionPart}") * multiplier
                };
            } else {
                if (intPart == "Infinity") {
                    return new HasmNumberToken(state) {
                        Value = multiplier == 1.0 ? double.PositiveInfinity : double.NegativeInfinity
                    };
                }
                if (intPart == "NaN") {
                    return new HasmNumberToken(state) {
                        Value = double.NaN
                    };
                }
                return new HasmNumberToken(state) {
                    Value = double.Parse(intPart) * multiplier
                };
            }
        }
    }
}
