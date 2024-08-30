using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;

namespace Hasmer {
    /// <summary>
    /// Represents an item whose parsing format is defined in JSON, whose value corresponds to a series of data in a Hermes bytecode file.
    /// </summary>
    public abstract class HbcEncodedItem {
        /// <summary>
        /// Converts the name of a type in JSON to the .NET type.
        /// </summary>
        private static Type GetTypeFromName(string name) => name switch {
            "UInt8" => typeof(byte),
            "UInt16" => typeof(ushort),
            "UInt32" => typeof(uint),
            "UInt64" => typeof(ulong),
            _ => throw new InvalidDataException("bad type: " + name),
        };

        /// <summary>
        /// Reads a value from the buffer given the type of the value.
        /// </summary>
        private static object ReadType(HbcReader reader, string type) => type switch {
            // the explicit cast to object is necessary, the C# compiler casts all the values to UInt64 before casting to object
            "UInt8" => (object)reader.ReadByte(),
            "UInt16" => (object)reader.ReadUInt16(),
            "UInt32" => (object)reader.ReadUInt32(),
            "UInt64" => (object)reader.ReadUInt64(),
            _ => throw new InvalidDataException("bad type: " + type),
        };

        /// <summary>
        /// Writes a value to the buffer given the type.
        /// </summary>
        private static void WriteType(HbcWriter writer, string type, object value) {
            switch (type) {
                case "UInt8":
                    writer.Write((byte)value);
                    break;
                case "UInt16":
                    writer.Write((ushort)value);
                    break;
                case "UInt32":
                    writer.Write((uint)value);
                    break;
                case "UInt64":
                    writer.Write((ulong)value);
                    break;
                default:
                    throw new InvalidDataException("bad type: " + type);
            }
        }

        /// <summary>
        /// Writes a complex value given the definition and corresponding object.
        /// </summary>
        public static void WriteFromDefinition(HbcWriter writer, JToken def, object value) {
            if (def.Type == JTokenType.Array) {
                JArray tuple = (JArray)def;
                string type = (string)tuple[0];

                if (tuple[1].Type == JTokenType.Integer) {
                    int toWrite = (int)tuple[1];
                    if (type == "Bit") {
                        writer.WriteBits((uint)value, toWrite);
                    } else {
                        Array array = (Array)value;
                        if (array.Length != toWrite) {
                            throw new InvalidDataException("array is wrong length");
                        }
                        for (int i = 0; i < toWrite; i++) {
                            WriteType(writer, type, array.GetValue(i));
                        }
                    }
                } else if (tuple[1].Type == JTokenType.String) {
                    // TODO
                    throw new NotImplementedException();
                } else {
                    throw new InvalidDataException("bad tuple definition");
                }
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
                    throw new InvalidDataException("bad tuple definition");
                }
            } else {
                string type = (string)def;
                return ReadType(reader, type);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a complex value from a stream given its definition.
        /// </summary>
        public static T Decode<T>(HbcReader reader, JObject obj) where T : HbcEncodedItem, new() {
            T decoded = new T();

            foreach (JProperty property in obj.Properties()) {
                PropertyInfo info = typeof(T).GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                object value = ReadFromDefinition(reader, property.Value);
                try {
                    info.SetValue(decoded, value);
                } catch (Exception e) {
                    Console.WriteLine($"Failed to set {info.PropertyType.FullName} property '{typeof(T).FullName}.{property.Name}' (expecting {property.Value}, read {value.GetType().FullName} {value}): {e}");
                    Environment.Exit(1);
                }
            }

            return decoded;
        }

        /// <summary>
        /// Encodes a complex value to a stream given its definition.
        /// </summary>
        public static void Encode<T>(HbcWriter writer, JObject obj, T item) where T : HbcEncodedItem {
            foreach (JProperty property in obj.Properties()) {
                PropertyInfo info = typeof(T).GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                WriteFromDefinition(writer, property.Value, info.GetValue(item));
            }
        }
    }
}
