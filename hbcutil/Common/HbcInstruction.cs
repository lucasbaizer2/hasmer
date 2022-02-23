using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HbcUtil {
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
        /// </summary>
        public PrimitiveValue Value { private get; set; }

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
                HbcInstructionOperandType.UInt16S => reader.ReadInt16(),
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
                HbcInstructionOperandType.UInt8S => (T)(object)file.StringTable[GetValue<byte>()],
                HbcInstructionOperandType.UInt16S => (T)(object)file.StringTable[GetValue<ushort>()],
                HbcInstructionOperandType.UInt32S => (T)(object)file.StringTable[GetValue<uint>()],
                _ => GetValue<T>()
            };
        }

        /// <summary>
        /// Converts a double to a string, catching edge cases.
        /// </summary>
        private string ToDoubleString(double d) {
            if (double.IsNegativeInfinity(d)) {
                return "-Infinity";
            } else if (double.IsPositiveInfinity(d)) {
                return "Infinity";
            } else if (double.IsNaN(d)) {
                return "NaN";
            }

            string format = new string('#', 324); // https://stackoverflow.com/a/14964797
            return d.ToString("0." + format);
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
                HbcInstructionOperandType.Double => ToDoubleString(GetValue<double>()),
                HbcInstructionOperandType.UInt8S => $"\"{StringEscape.Escape(file.StringTable[GetValue<byte>()])}\"",
                HbcInstructionOperandType.UInt16S => $"\"{StringEscape.Escape(file.StringTable[GetValue<ushort>()])}\"",
                HbcInstructionOperandType.UInt32S => $"\"{StringEscape.Escape(file.StringTable[GetValue<uint>()])}\"",
                _ => throw new InvalidOperationException("invalid operand type"),
            };
        }
    }

    /// <summary>
    /// Represents an instruction in Hermes bytecode.
    /// </summary>
    public class HbcInstruction {
        /// <summary>
        /// The one-byte instruction opcode.
        /// </summary>
        public byte Opcode { get; set; }
        /// <summary>
        /// The offset of the instruction relative to the function definition.
        /// </summary>
        public uint Offset { get; set; }
        /// <summary>
        /// The total length of the instruction and its operands in bytes.
        /// </summary>
        public uint Length { get; set; }
        /// <summary>
        /// The operands passed to the instruction.
        /// </summary>
        public List<HbcInstructionOperand> Operands { get; set; }

        /// <summary>
        /// Converts the instruction into a human-readable disassembly format used for debugging.
        /// </summary>
        public string ToDisassembly(HbcFile file) {
            string name = file.BytecodeFormat.Definitions[Opcode].Name;
            string operandString = string.Join(", ", Operands.Select(x => x.ToDisassembly(file)));

            return $"{name} {operandString}".Trim();
        }
    }
}
