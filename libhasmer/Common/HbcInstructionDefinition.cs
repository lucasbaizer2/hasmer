using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hasmer {
    /// <summary>
    /// Represents the definition of an instruction in a JSON bytecode definitions file.
    /// </summary>
    public class HbcInstructionDefinition {
        /// <summary>
        /// The opcode of the instruction in the binary.
        /// </summary>
        [JsonProperty]
        public int Opcode { get; set; }
        /// <summary>
        /// The human-readable name of the instruction.
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }
        /// <summary>
        /// The types of all the operands the operand handles.
        /// </summary>
        [JsonProperty]
        public List<HbcInstructionOperandType> OperandTypes { get; set; }
        /// <summary>
        /// true if the operation is a jumping instruction (i.e. changes the current instruction being executed), or otherwise false.
        /// </summary>
        [JsonProperty]
        public bool IsJump { get; set; }
        /// <summary>
        /// The index in the abstract definition table of the abstract form of the instruction, or null if the instruction does not have an abstract form.
        /// </summary>
        [JsonProperty]
        public int? AbstractDefinition { get; set; }

        public int TotalSize => 1 + OperandTypes.Select(opType => opType.GetSizeof()).Sum();
    }
}
