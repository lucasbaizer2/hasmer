using System;
using System.IO;

namespace Hasmer {
    /// <summary>
    /// Represents an operand of Hermes bytecode instruction.
    /// </summary>
    public class HbcInstructionOperand {
        /// <summary>
        /// The type of operand that is represented by the object.
        /// </summary>
        public HbcInstructionOperandType Type { get; set; }

        /// <summary>
        /// The raw value of the object, represented as a PrimitiveValue to preserve type information.
        /// <br />
        /// Is is highly recommended to use <see cref="GetValue{T}"/> and <see cref="GetResolvedValue{T}(HbcFile)"/>
        /// instead of accessing this property directly.
        /// Use with caution.
        /// </summary>
        public PrimitiveValue Value { get; set; }

        /// <summary>
        /// Writes the operand to a stream of binary data.
        /// </summary>
        public void ToWriter(BinaryWriter writer) {
            switch (Type) {
                case HbcInstructionOperandType.Reg8: writer.Write(GetValue<byte>()); break;
                case HbcInstructionOperandType.Reg32: writer.Write(GetValue<uint>()); break;
                case HbcInstructionOperandType.UInt8: writer.Write(GetValue<byte>()); break;
                case HbcInstructionOperandType.UInt16: writer.Write(GetValue<ushort>()); break;
                case HbcInstructionOperandType.UInt32: writer.Write(GetValue<uint>()); break;
                case HbcInstructionOperandType.Addr8: writer.Write(GetValue<sbyte>()); break;
                case HbcInstructionOperandType.Addr32: writer.Write(GetValue<int>()); break;
                case HbcInstructionOperandType.Imm32: writer.Write(GetValue<uint>()); break;
                case HbcInstructionOperandType.Double: writer.Write(GetValue<double>()); break;
                case HbcInstructionOperandType.UInt8S: writer.Write(GetValue<byte>()); break;
                case HbcInstructionOperandType.UInt16S: writer.Write(GetValue<ushort>()); break;
                case HbcInstructionOperandType.UInt32S: writer.Write(GetValue<uint>()); break;
                default: throw new InvalidOperationException("invalid operand type");
            }
        }

        /// <summary>
        /// Reads the operand from a stream of binary data.
        /// </summary>
        public static HbcInstructionOperand FromReader(BinaryReader reader, HbcInstructionOperandType type) {
            object rawValue = type switch {
                HbcInstructionOperandType.Reg8 => reader.ReadByte(),
                HbcInstructionOperandType.Reg32 => reader.ReadUInt32(),
                HbcInstructionOperandType.UInt8 => reader.ReadByte(),
                HbcInstructionOperandType.UInt16 => reader.ReadUInt16(),
                HbcInstructionOperandType.UInt32 => reader.ReadUInt32(),
                HbcInstructionOperandType.Addr8 => reader.ReadSByte(),
                HbcInstructionOperandType.Addr32 => reader.ReadInt32(),
                HbcInstructionOperandType.Imm32 => reader.ReadUInt32(),
                HbcInstructionOperandType.Double => reader.ReadDouble(),
                HbcInstructionOperandType.UInt8S => reader.ReadByte(),
                HbcInstructionOperandType.UInt16S => reader.ReadUInt16(),
                HbcInstructionOperandType.UInt32S => reader.ReadUInt32(),
                _ => throw new InvalidOperationException("invalid operand type"),
            };
            return new HbcInstructionOperand {
                Type = type,
                Value = new PrimitiveValue(rawValue)
            };
        }

        /// <summary>
        /// Returns the value as the given type.
        /// </summary>
        public T GetValue<T>() {
            return Value.GetValue<T>();
        }

        /// <summary>
        /// Returns the value, or is the value is a string, the string the value points to.
        /// </summary>
        public T GetResolvedValue<T>(HbcFile file) {
            return Type switch {
                HbcInstructionOperandType.UInt8S => (T)(object)file.GetStringTableEntry((int)GetValue<byte>()),
                HbcInstructionOperandType.UInt16S => (T)(object)file.GetStringTableEntry((int)GetValue<ushort>()),
                HbcInstructionOperandType.UInt32S => (T)(object)file.GetStringTableEntry((int)GetValue<uint>()),
                _ => GetValue<T>()
            };
        }

        /// <summary>
        /// Converts the operand to a human-readable format used for disassembly.
        /// </summary>
        public string ToDisassembly(HbcFile file) {
            return Type switch {
                HbcInstructionOperandType.Reg8 => $"r{GetValue<byte>()}",
                HbcInstructionOperandType.Reg32 => $"r{GetValue<uint>()}",
                HbcInstructionOperandType.UInt8 => GetValue<byte>().ToString(),
                HbcInstructionOperandType.UInt16 => GetValue<ushort>().ToString(),
                HbcInstructionOperandType.UInt32 => GetValue<uint>().ToString(),
                HbcInstructionOperandType.Addr8 => $"Addr8({GetValue<sbyte>()})",
                HbcInstructionOperandType.Addr32 => $"Addr32({GetValue<int>()})",
                HbcInstructionOperandType.Imm32 => GetValue<uint>().ToString(),
                HbcInstructionOperandType.Double => StringEscape.DoubleToString(GetValue<double>()),
                HbcInstructionOperandType.UInt8S => file.GetStringTableEntry((int)GetValue<byte>()).Printable,
                HbcInstructionOperandType.UInt16S => file.GetStringTableEntry((int)GetValue<ushort>()).Printable,
                HbcInstructionOperandType.UInt32S => file.GetStringTableEntry((int)GetValue<uint>()).Printable,
                _ => throw new InvalidOperationException("invalid operand type"),
            };
        }
    }
}
