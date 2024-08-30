
namespace Hasmer.Assembler.Parser {
    /// <summary>
    /// Represents an integer, either a 4-byte signed integer or a 4-byte unsigned integer specifically.
    /// </summary>
    public class HasmIntegerToken : HasmLiteralToken {
        /// <summary>
        /// The integer value. This is stored a long to be able to handle both int and uint types.
        /// </summary>
        private long Value { get; set; }

        /// <summary>
        /// Returns a PrimitiveValue whose underlying byte count is equal to the smallest multiple of 2 that can hold <see cref="Value" />.
        /// </summary>
        public PrimitiveValue GetCompactValue(bool signed) {
            if (signed) {
                if (Value >= sbyte.MinValue && Value <= sbyte.MaxValue) {
                    return new PrimitiveValue((sbyte)Value);
                } else if (Value >= short.MinValue && Value <= short.MaxValue) {
                    return new PrimitiveValue((short)Value);
                } else if (Value >= int.MinValue && Value <= int.MaxValue) {
                    return new PrimitiveValue((int)Value);
                }
            } else {
                if (Value >= byte.MinValue && Value <= byte.MaxValue) {
                    return new PrimitiveValue((byte)Value);
                } else if (Value >= ushort.MinValue && Value <= ushort.MaxValue) {
                    return new PrimitiveValue((ushort)Value);
                } else if (Value >= uint.MinValue && Value <= uint.MaxValue) {
                    return new PrimitiveValue((uint)Value);
                }
            }

            throw new HasmParserException(this, $"Value is too large to store in a PrimitiveValue: {Value}");
        }

        /// <summary>
        /// Returns a PrimitiveValue containing <see cref="Value" /> as an integer of <see cref="size" /> width.
        /// If <see cref="Value" /> cannot be represented in <see cref="size" /> bytes, an exception is thrown.
        /// </summary>
        public PrimitiveValue GetValue(bool signed, int size) {
            if (signed) {
                if (size == 1) {
                    if (Value < sbyte.MinValue || Value > sbyte.MaxValue) {
                        throw new HasmParserException(this, $"integer is not sbyte: {Value}");
                    }
                    return new PrimitiveValue((sbyte)Value);
                } else if (size == 2) {
                    if (Value < short.MinValue || Value > short.MaxValue) {
                        throw new HasmParserException(this, $"integer is not short: {Value}");
                    }
                    return new PrimitiveValue((short)Value);
                } else if (size == 4) {
                    if (Value < int.MinValue || Value > int.MaxValue) {
                        throw new HasmParserException(this, $"integer is not int: {Value}");
                    }
                    return new PrimitiveValue((int)Value);
                }
            } else {
                if (size == 1) {
                    if (Value < byte.MinValue || Value > byte.MaxValue) {
                        throw new HasmParserException(this, $"integer is not byte: {Value}");
                    }
                    return new PrimitiveValue((byte)Value);
                } else if (size == 2) {
                    if (Value < ushort.MinValue || Value > ushort.MaxValue) {
                        throw new HasmParserException(this, $"integer is not ushort: {Value}");
                    }
                    return new PrimitiveValue((ushort)Value);
                } else if (size == 4) {
                    if (Value < uint.MinValue || Value > uint.MaxValue) {
                        throw new HasmParserException(this, $"integer is not int: {Value}");
                    }
                    return new PrimitiveValue((uint)Value);
                }
            }

            throw new HasmParserException(this, $"invalid integer size: {size}");
        }

        /// <summary>
        /// Returns the value as a 4-byte unsigned integer; throws an exception if the value is not in bounds.
        /// </summary>
        public uint GetValueAsUInt32() {
            if (Value < uint.MinValue || Value > uint.MaxValue) {
                throw new HasmParserException(this, $"integer is not uint: {Value}");
            }
            return (uint)Value;
        }

        /// <summary>
        /// Returns the value as a 4-byte signed integer; throws an exception if the value is not in bounds.
        /// </summary>
        public int GetValueAsInt32() {
            if (Value < int.MinValue || Value > int.MaxValue) {
                throw new HasmParserException(this, $"integer is not int: {Value}");
            }
            return (int)Value;
        }

        public HasmIntegerToken(HasmStringStreamState state, long value) : base(state) {
            Value = value;
        }
    }
}
