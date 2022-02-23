﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HbcUtil {
    /// <summary>
    /// The flags a function can have. Note that the Prohibit* flags cannot be used in conjunction with Enum.HasFlag because ProhibitCall is 0.
    /// </summary>
    public enum HbcFuncHeaderFlags : byte {
        ProhibitCall = 0x00,
        ProhibitConstruct = 0x01,
        ProhibitNone = 0x02,
        StrictMode = 0x04,
        HasExceptionHandler = 0x08,
        HasDebugInfo = 0x10,
        Overflowed = 0x20
    }

    /// <summary>
    /// Represents a function header definition in a Hermes bytecode file.
    /// </summary>
    public class HbcFuncHeader : HbcEncodedItem {
        /// <summary>
        /// The sequential ID of the function.
        /// </summary>
        public uint FunctionId { get; set; }
        /// <summary>
        /// The offset in the HbcFile of the function.
        /// </summary>
        public uint Offset { get; set; }
        /// <summary>
        /// The amount of parameters that the function takes.
        /// </summary>
        public uint ParamCount { get; set; }
        /// <summary>
        /// The amount of bytes that the function's bytecode takes up.
        /// </summary>
        public uint BytecodeSizeInBytes { get; set; }
        /// <summary>
        /// The index in the string table of the function's name (i.e. HbcFile.StringTable[FunctionName]).
        /// </summary>
        public uint FunctionName { get; set; }
        /// <summary>
        /// The offset in the HbcFile of additional (i.e. debug) information about the function.
        /// </summary>
        public uint InfoOffset { get; set; }
        /// <summary>
        /// The amount of registers that the function has.
        /// </summary>
        public uint FrameSize { get; set; }
        /// <summary>
        /// The amount of symbols that the function has.
        /// </summary>
        public uint EnvironmentSize { get; set; }
        public uint HighestReadCacheIndex { get; set; }
        public uint HighestWriteCacheIndex { get; set; }
        public HbcFuncHeaderFlags Flags { get; set; }
        [JsonIgnore]
        public HbcFile DeclarationFile { get; set; }

        public virtual HbcFuncHeader GetAssemblerHeader() {
            return this;
        }

        /// <summary>
        /// Disassembles the function, parsing the bytecode into an <see cref="HbcInstruction">HbcInstruction</see> object for each instruction.
        /// </summary>
        /// <returns>An enumerator that yields each instruction in the function.</returns>
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