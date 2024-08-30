using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hasmer {
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
