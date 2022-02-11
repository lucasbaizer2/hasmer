using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HbcUtil {
    public enum HbcFuncHeaderFlags : byte {
        ProhibitCall = 0x00,
        ProhibitConstruct = 0x01,
        ProhibitNone = 0x02,
        StrictMode = 0x04,
        HasExceptionHandler = 0x08,
        HasDebugInfo = 0x10,
        Overflowed = 0x20
    }

    public class HbcFuncHeader : HbcEncodedItem {
        public uint FunctionId { get; set; }
        public uint Offset { get; set; }
        public uint ParamCount { get; set; }
        public uint BytecodeSizeInBytes { get; set; }
        public uint FunctionName { get; set; }
        public uint InfoOffset { get; set; }
        public uint FrameSize { get; set; }
        public uint EnvironmentSize { get; set; }
        public uint HighestReadCacheIndex { get; set; }
        public uint HighestWriteCacheIndex { get; set; }
        public HbcFuncHeaderFlags Flags { get; set; }
        [JsonIgnore]
        public HbcFile DeclarationFile { get; set; }

        public virtual HbcFuncHeader GetAssemblerHeader() {
            return this;
        }

        public IEnumerable<HbcInstruction> Disassemble() {
            uint offset = GetAssemblerHeader().Offset - DeclarationFile.InstructionOffset;

            using MemoryStream ms = new MemoryStream(DeclarationFile.Instructions);
            ms.Position = offset;

            using BinaryReader reader = new BinaryReader(ms);
            while (ms.Position < offset + BytecodeSizeInBytes) {
                long startPos = ms.Position;
                byte opcodeValue = reader.ReadByte();
                HbcInstructionDefinition def = DeclarationFile.BytecodeFormat.Definitions[opcodeValue];

                List<HbcInstructionOperand> operands = new List<HbcInstructionOperand>(def.OperandTypes.Count);
                foreach (HbcInstructionOperandType type in def.OperandTypes) {
                    operands.Add(HbcInstructionOperand.FromReader(reader, type));
                }

                long endPos = ms.Position;
                yield return new HbcInstruction {
                    Opcode = opcodeValue,
                    Operands = operands,
                    Offset = (uint)ms.Position - offset,
                    Length = (uint)(endPos - startPos)
                };
            }
        }
    }
}
