using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Hasmer {
    /// <summary>
    /// Represents the type of an operand.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HbcInstructionOperandType {
        /// <summary>
        /// The operand is a one-byte register reference.
        /// </summary>
        Reg8,
        /// <summary>
        /// The operand is a four-byte register reference.
        /// </summary>
        Reg32,
        /// <summary>
        /// The operand is an unsigned byte.
        /// </summary>
        UInt8,
        /// <summary>
        /// The operand is an unsigned two-byte integer.
        /// </summary>
        UInt16,
        /// <summary>
        /// The operand is an unsigned four-byte integer.
        /// </summary>
        UInt32,
        /// <summary>
        /// The operand is a one-byte code address reference.
        /// </summary>
        Addr8,
        /// <summary>
        /// The operand is a four-byte code address reference.
        /// </summary>
        Addr32,
        /// <summary>
        /// The operand is a four-byte unsigned integer.
        /// </summary>
        Imm32,
        /// <summary>
        /// The operand is an eight-byte IEEE754 floating-point value.
        /// </summary>
        Double,
        /// <summary>
        /// The operand is a one-byte reference to the string table.
        /// </summary>
        UInt8S,
        /// <summary>
        /// The operand is a two-byte reference to the string table.
        /// </summary>
        UInt16S,
        /// <summary>
        /// The operand is a four-byte reference to the string table.
        /// </summary>
        UInt32S
    }

    public static class HbcInstructionOperandTypeImpl {
        public static int GetSizeof(this HbcInstructionOperandType opType) {
            return opType switch {
                HbcInstructionOperandType.Reg8 or HbcInstructionOperandType.UInt8 or HbcInstructionOperandType.Addr8 or HbcInstructionOperandType.UInt8S => 1,
                HbcInstructionOperandType.UInt16 or HbcInstructionOperandType.UInt16S => 2,
                HbcInstructionOperandType.Reg32 or HbcInstructionOperandType.UInt32 or HbcInstructionOperandType.Addr32 or HbcInstructionOperandType.Imm32 or HbcInstructionOperandType.UInt32S => 4,
                HbcInstructionOperandType.Double => 8,
                _ => throw new Exception("unreachable"),
            };
        }

        public static bool CanStoreInteger(this HbcInstructionOperandType opType, ulong integer) {
            switch (opType) {
                case HbcInstructionOperandType.Reg8:
                case HbcInstructionOperandType.UInt8:
                case HbcInstructionOperandType.UInt8S:
                    return integer <= byte.MaxValue;
                case HbcInstructionOperandType.UInt16:
                case HbcInstructionOperandType.UInt16S:
                    return integer <= ushort.MaxValue;
                case HbcInstructionOperandType.Addr8:
                    return (long)integer >= sbyte.MinValue && (long)integer <= sbyte.MaxValue;
                case HbcInstructionOperandType.Addr32:
                    return (long)integer >= int.MinValue && (long)integer <= int.MaxValue;
                case HbcInstructionOperandType.Imm32:
                case HbcInstructionOperandType.Reg32:
                case HbcInstructionOperandType.UInt32:
                case HbcInstructionOperandType.UInt32S:
                    return integer <= uint.MaxValue;
                default:
                    throw new Exception($"invalid operand type to store integer: {opType}");
            }
        }
    }
}
