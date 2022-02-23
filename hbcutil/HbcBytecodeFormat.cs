using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace HbcUtil {
    /// <summary>
    /// Represents the type of an operand.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HbcInstructionOperandType {
        /// <summary>
        /// The operand is a one-byte register reference.
        /// </summary>
        Reg8,
        /// <summary>
        /// The operand is a four-byte register reference.
        /// </summary>
        Reg32,
        /// <summary>
        /// The operand is an unsigned byte.
        /// </summary>
        UInt8,
        /// <summary>
        /// The operand is an unsigned two-byte integer.
        /// </summary>
        UInt16,
        /// <summary>
        /// The operand is an unsigned four-byte integer.
        /// </summary>
        UInt32,
        /// <summary>
        /// The operand is a one-byte code address reference.
        /// </summary>
        Addr8,
        /// <summary>
        /// The operand is a four-byte code address reference.
        /// </summary>
        Addr32,
        /// <summary>
        /// The operand is a four-byte unsigned integer.
        /// </summary>
        Imm32,
        /// <summary>
        /// The operand is an eight-byte IEEE754 floating-point value.
        /// </summary>
        Double,
        /// <summary>
        /// The operand is a one-byte reference to the string table.
        /// </summary>
        UInt8S,
        /// <summary>
        /// The operand is a two-byte reference to the string table.
        /// </summary>
        UInt16S,
        /// <summary>
        /// The operand is a four-byte reference to the string table.
        /// </summary>
        UInt32S
    }

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
    }

    public class HbcBytecodeFormat {
        /// <summary>
        /// The Hermes bytecode version this format is relevant to.
        /// </summary>
        [JsonProperty]
        public int Version { get; set; }

        /// <summary>
        /// The definitions of all opcodes available for the bytecode version.
        /// This list can be indexed by the encoded value of the opcode.
        /// </summary>
        [JsonProperty]
        public List<HbcInstructionDefinition> Definitions { get; set; }
    }
}
