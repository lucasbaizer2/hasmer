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

    [JsonConverter(typeof(HbcInstructionDefinitionConverter))]
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

    public class HbcInstructionDefinitionConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            JObject obj = JObject.Load(reader);

            int version = (int)obj["Version"];
            JObject opcodesObject = (JObject)obj["Opcodes"];
            List<JProperty> properties = opcodesObject.Properties().ToList();
            List<HbcInstructionDefinition> definitions = new List<HbcInstructionDefinition>(properties.Count);

            for (int i = 0; i < properties.Count; i++) {
                JProperty prop = properties[i];
                string opcodeName = prop.Name;
                List<HbcInstructionOperandType> operandTypes = prop.Value.ToObject<List<HbcInstructionOperandType>>();

                definitions.Add(new HbcInstructionDefinition {
                    Opcode = i,
                    Name = opcodeName,
                    OperandTypes = operandTypes
                });
            }

            return new HbcBytecodeFormat {
                Version = version,
                Definitions = definitions
            };
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => false;
    }
}
