using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        /// The functions that are declared in the file.
        /// </summary>
        public List<HbcFunctionBuilder> Functions { get; set; }

        /// <summary>
        /// Represents the mappings between each function ID (key)
        /// and their offset in the instructions buffer (value).
        /// </summary>
        private Dictionary<uint, uint> FunctionOffsets;

        /// <summary>
        /// Represents the string table.
        /// The key is the string and the value is the ID of the string.
        /// The ID is a sequential value, incremented for each new string.
        /// </summary>
        private Dictionary<string, uint> StringTable = new Dictionary<string, uint>();

        /// <summary>
        /// Constructs a new HbcFileBuilder given the bytecode format
        /// </summary>
        public HbcFileBuilder(HbcBytecodeFormat format) {
            Format = format;
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

        /// <summary>
        /// Serializes the instructions of every function to a buffer sequentially.
        /// </summary>
        private void BuildBytecode() {
            FunctionOffsets = new Dictionary<uint, uint>(Functions.Count);

            using MemoryStream ms = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(ms);

            foreach (HbcFunctionBuilder builder in Functions) {
                FunctionOffsets[builder.FunctionId] = (uint)ms.Position;
                /*
                foreach (HbcInstruction insn in builder.Instructions) {
                    writer.Write(insn.Opcode);
                    foreach (HbcInstructionOperand operand in insn.Operands) {
                        if (operand.Value.TypeCode == TypeCode.String) {
                            // convert string operands to their string ID
                            // TODO
                        }
                        operand.ToWriter(writer);
                    }
                }
                */
            }
        }

        public HbcFile Build() {
            HbcHeader header = new HbcHeader {
                GlobalCodeIndex = 0,
                Magic = HbcHeader.HBC_MAGIC_HEADER,
                Version = Format.Version,
                SourceHash = new byte[20],
                Padding = new byte[31],
                FunctionCount = (uint)Functions.Count
            };

            BuildBytecode();

            HbcFile file = new HbcFile();
            file.BytecodeFormat = Format;

            return file;
        }
    }
}
