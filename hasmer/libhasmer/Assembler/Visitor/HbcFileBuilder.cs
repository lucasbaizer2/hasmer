using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Hasmer.Assembler.Parser;

namespace Hasmer.Assembler.Visitor {
    /// <summary>
    /// Represents data for a <see cref="HbcFile"/> that is progressively created by the assembler.
    /// <br />
    /// Once all the data has been added, an HbcFile can be built using the <see cref="Build"/> method.
    /// </summary>
    public class HbcFileBuilder {
        /// <summary>
        /// The bytecode format being used for the file.
        /// </summary>
        public HbcBytecodeFormat Format { get; set; }

        /// <summary>
        /// True if the bytecode is being written as-is (i.e. literally), false if the bytecode should be optimized into variants.
        /// See <see cref="HbcDisassembler.IsExact"/> for more information
        /// </summary>
        public bool IsExact { get; set; }

        /// <summary>
        /// The functions that are declared in the file.
        /// </summary>
        public List<HbcFunctionBuilder> Functions { get; set; }

        /// <summary>
        /// The file being built.
        /// </summary>
        private HbcFile File { get; set; }

        /// <summary>
        /// Represents the string table.
        /// The key is the string and the value is the ID of the string.
        /// The ID is a sequential value, incremented for each new string.
        /// </summary>
        private Dictionary<string, uint> StringTable = new Dictionary<string, uint>();

        /// <summary>
        /// Constructs a new HbcFileBuilder given the bytecode format
        /// </summary>
        public HbcFileBuilder(HbcBytecodeFormat format, bool isExact) {
            Format = format;
            IsExact = isExact;
        }

        /// <summary>
        /// Returns the ID of the given string in the <see cref="StringTable"/>.
        /// If the string is not already present in the string table,
        /// it is added and the ID of the newly added string is returned.
        /// </summary>
        private uint GetStringId(string str) {
            if (StringTable.ContainsKey(str)) {
                return StringTable[str];
            }

            uint newId = (uint)StringTable.Count;
            StringTable[str] = newId;
            return newId;
        }

        private void WriteInstruction(BinaryWriter writer, HasmInstructionToken insn) {
            string insnName = insn.Instruction;
            if (!IsExact) {
            }

            HbcInstructionDefinition def = Format.Definitions.Find(def => def.Name == insnName);
            if (def == null) {
                throw new HasmParserException(insn.Line, insn.Column, $"unknown instruction: {insnName}");
            }
            writer.Write((byte)def.Opcode);

            for (int i = 0; i < def.OperandTypes.Count; i++) {
                HasmOperandToken operand = insn.Operands[i];
                HbcInstructionOperandType type = def.OperandTypes[i];

                if (operand.Value.TypeCode == TypeCode.String) {
                    operand.Value = new PrimitiveValue(); // convert string to ID
                }
            }
        }

        /// <summary>
        /// Serializes the instructions of every function to a buffer sequentially.
        /// </summary>
        private void BuildBytecode() {
            using MemoryStream ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);

            foreach (HbcFunctionBuilder builder in Functions) {
                File.SmallFuncHeaders[builder.FunctionId].Offset = (uint)ms.Position;
                foreach (HasmInstructionToken insn in builder.Instructions) {
                    WriteInstruction(writer, insn);
                }
                File.SmallFuncHeaders[builder.FunctionId].BytecodeSizeInBytes = (uint)ms.Position - File.SmallFuncHeaders[builder.FunctionId].Offset;
            }
        }

        public HbcFile Build() {
            File.Header = new HbcHeader {
                GlobalCodeIndex = 0,
                Magic = HbcHeader.HBC_MAGIC_HEADER,
                Version = Format.Version,
                SourceHash = new byte[20],
                Padding = new byte[31],
                FunctionCount = (uint)Functions.Count
            };

            BuildBytecode();

            // TODO

            return File;
        }
    }
}
