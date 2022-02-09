using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HbcUtil {
    public class HbcInstructionOperand {
        public HbcInstructionOperandType Type { get; set; }
        public object RawValue { get; set; }

        public static HbcInstructionOperand FromReader(BinaryReader reader, HbcInstructionOperandType type) {
            object rawValue = type switch {
                HbcInstructionOperandType.Reg8 => reader.ReadByte(),
                HbcInstructionOperandType.Reg32 => reader.ReadUInt32(),
                HbcInstructionOperandType.UInt8 => reader.ReadByte(),
                HbcInstructionOperandType.UInt16 => reader.ReadUInt16(),
                HbcInstructionOperandType.UInt32 => reader.ReadUInt32(),
                HbcInstructionOperandType.Addr8 => reader.ReadSByte(),
                HbcInstructionOperandType.Addr32 => reader.ReadUInt32(),
                HbcInstructionOperandType.Imm32 => reader.ReadUInt32(),
                HbcInstructionOperandType.Double => reader.ReadDouble(),
                HbcInstructionOperandType.UInt8S => reader.ReadByte(),
                HbcInstructionOperandType.UInt16S => reader.ReadInt16(),
                HbcInstructionOperandType.UInt32S => reader.ReadUInt32(),
                _ => throw new NotImplementedException(),
            };
            return new HbcInstructionOperand {
                Type = type,
                RawValue = rawValue
            };
        }

        public T GetValue<T>() {
            if (RawValue is not T) {
                throw new Exception($"Operand {Type} cannot be represented as {nameof(T)}");
            }
            return (T)RawValue;
        }

        public override string ToString() {
            return $"{Type}({RawValue})";
        }
    }

    public class HbcInstruction {
        public byte Opcode { get; set; }
        public List<HbcInstructionOperand> Operands { get; set; }
    }
}
