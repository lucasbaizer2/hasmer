using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HbcUtil {
    public class HbcFuncHeader : HbcEncodedItem {
        public uint Offset { get; set; }
        public uint ParamCount { get; set; }
        public uint BytecodeSizeInBytes { get; set; }
        public uint FunctionName { get; set; }
        public uint InfoOffset { get; set; }
        public uint FrameSize { get; set; }
        public uint EnvironmentSize { get; set; }
        public uint HighestReadCacheIndex { get; set; }
        public uint HighestWriteCacheIndex { get; set; }
        public byte Flags { get; set; }
        [JsonIgnore]
        public HbcFile DeclarationFile { get; set; }

        public IEnumerable<HbcInstruction> Disassemble() {
            using MemoryStream ms = new MemoryStream(DeclarationFile.Instructions);
            ms.Position = DeclarationFile.InstructionOffset - Offset;

            using BinaryReader reader = new BinaryReader(ms);
            while (ms.Position < DeclarationFile.InstructionOffset - Offset + BytecodeSizeInBytes) {
                byte opcodeValue = reader.ReadByte();
                Console.WriteLine(opcodeValue);
                HbcInstructionDefinition def = DeclarationFile.BytecodeFormat.Definitions[opcodeValue];

                List<HbcInstructionOperand> operands = new List<HbcInstructionOperand>(def.OperandTypes.Count);
                foreach (HbcInstructionOperandType type in def.OperandTypes) {
                    operands.Add(HbcInstructionOperand.FromReader(reader, type));
                }

                yield return new HbcInstruction {
                    Opcode = opcodeValue,
                    Operands = operands
                };
            }
        }
    }
}
