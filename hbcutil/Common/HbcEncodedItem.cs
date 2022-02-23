using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;

namespace HbcUtil {
    /// <summary>
    /// Represents an item whose parsing format is defined in JSON, whose value corresponds to a series of data in a Hermes bytecode file.
    /// </summary>
    public abstract class HbcEncodedItem {
        /// <summary>
        /// Converts the name of a type in JSON to the .NET type.
        /// </summary>
        private static Type GetTypeFromName(string name) {
            switch (name) {
                case "UInt8":
                    return typeof(byte);
                case "UInt16":
                    return typeof(ushort);
                case "UInt32":
                    return typeof(uint);
                case "UInt64":
                    return typeof(ulong);
                default:
                    throw new Exception("bad type: " + name);
            }
        }

        /// <summary>
        /// Reads a value from the buffer given the type of the value.
        /// </summary>
        private static object ReadType(HbcReader reader, string type) {
            if (type == "UInt8") {
                return reader.ReadByte();
            } else if (type == "UInt16") {
                return reader.ReadUInt16();
            } else if (type == "UInt32") {
                return reader.ReadUInt32();
            } else if (type == "UInt64") {
                return reader.ReadUInt64();
            } else {
                throw new Exception("bad type: " + type);
            }
        }

        /// <summary>
        /// Writes a value to the buffer given the type.
        /// </summary>
        private static void WriteType(HbcWriter writer, string type, object value) {
            if (type == "UInt8") {
                writer.Write((byte)value);
            } else if (type == "UInt16") {
                writer.Write((ushort)value);
            } else if (type == "UInt32") {
                writer.Write((uint)value);
            } else if (type == "UInt64") {
                writer.Write((ulong)value);
            } else {
                throw new Exception("bad type: " + type);
            }
        }

        /// <summary>
        /// Writes a complex value given the definition and corresponding object.
        /// </summary>
        public static void WriteFromDefinition(HbcWriter writer, JToken def, object value) {
            if (def.Type == JTokenType.Array) {

            } else {
                string type = (string)def;
                WriteType(writer, type, value);
            }
        }

        /// <summary>
        /// Reads a simple value given its definition.
        /// </summary>
        public static object ReadFromDefinition(HbcReader reader, JToken def) {
            if (def.Type == JTokenType.Array) {
                JArray tuple = (JArray)def;
                string type = (string)tuple[0];

                if (tuple[1].Type == JTokenType.Integer) {
                    int toRead = (int)tuple[1];
                    if (type == "Bit") {
                        return reader.ReadBits(toRead);
                    } else {
                        Array array = Array.CreateInstance(GetTypeFromName(type), toRead);
                        for (int i = 0; i < toRead; i++) {
                            array.SetValue(ReadType(reader, type), i);
                        }
                        return array;
                    }
                } else if (tuple[1].Type == JTokenType.String) {
                    // TODO
                    throw new NotImplementedException();
                } else {
                    throw new Exception("bad tuple definition");
                }
            } else {
                string type = (string)def;
                return ReadType(reader, type);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a complex value given its definition.
        /// </summary>
        public static T Decode<T>(HbcReader reader, JObject obj) where T : HbcEncodedItem, new() {
            T decoded = new T();

            foreach (JProperty property in obj.Properties()) {
                PropertyInfo info = typeof(T).GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                object value = ReadFromDefinition(reader, property.Value);
                info.SetValue(decoded, value);
            }

            return decoded;
        }

        /// <summary>
        /// Encodes a complex value given its definition.
        /// </summary>
        public static void Encode<T>(HbcWriter writer, JObject obj, T item) where T : HbcEncodedItem {
            foreach (JProperty property in obj.Properties()) {
                PropertyInfo info = typeof(T).GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                WriteFromDefinition(writer, property.Value, info.GetValue(item));
            }
        }
    }
}
