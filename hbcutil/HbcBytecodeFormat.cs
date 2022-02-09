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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HbcInstructionOperandType {
        Reg8,
        Reg32,
        UInt8,
        UInt16,
        UInt32,
        Addr8,
        Addr32,
        Imm32,
        Double,
        UInt8S,
        UInt16S,
        UInt32S
    }

    public class HbcInstructionDefinition {
        public int Opcode { get; set; }
        public string Name { get; set; }
        public List<HbcInstructionOperandType> OperandTypes { get; set; }
    }

    public class HbcBytecodeFormat {
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
