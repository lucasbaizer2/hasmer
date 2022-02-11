using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HbcUtil {
    public class HbcInstructionOperand {
        public HbcInstructionOperandType Type { get; set; }
        public PrimitiveValue Value { private get; set; }

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

        public T GetValue<T>() {
            return Value.GetValue<T>();
        }

        public T GetResolvedValue<T>(HbcFile file) {
            return Type switch {
                HbcInstructionOperandType.UInt8S => (T)(object)file.StringTable[GetValue<byte>()],
                HbcInstructionOperandType.UInt16S => (T)(object)file.StringTable[GetValue<ushort>()],
                HbcInstructionOperandType.UInt32S => (T)(object)file.StringTable[GetValue<uint>()],
                _ => GetValue<T>()
            };
        }

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
                HbcInstructionOperandType.Double => GetValue<double>().ToString(),
                HbcInstructionOperandType.UInt8S => $"\"{file.StringTable[GetValue<byte>()]}\"",
                HbcInstructionOperandType.UInt16S => $"\"{file.StringTable[GetValue<ushort>()]}\"",
                HbcInstructionOperandType.UInt32S => $"\"{file.StringTable[GetValue<uint>()]}\"",
                _ => throw new InvalidOperationException("invalid operand type"),
            };
        }
    }

    public class HbcInstruction {
        public byte Opcode { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }
        public List<HbcInstructionOperand> Operands { get; set; }

        public string ToDisassembly(HbcFile file) {
            string name = file.BytecodeFormat.Definitions[Opcode].Name;
            string operandString = string.Join(", ", Operands.Select(x => x.ToDisassembly(file)));

            return $"{name} {operandString}".Trim();
        }
    }
}
