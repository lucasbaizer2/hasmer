﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Hasmer {
    /// <summary>
    /// Represents a parsed Hermes bytecode file.
    /// </summary>
    public class HbcFile {
        /// <summary>
        /// The header of the file.
        /// </summary>
        public HbcHeader Header { get; set; }

        /// <summary>
        /// The bytecode format definition found, given the version of the Hermes bytecode in the file.
        /// </summary>
        public HbcBytecodeFormat BytecodeFormat { get; set; }

        /// <summary>
        /// The headers of all functions in the binary.
        /// </summary>
        public HbcSmallFuncHeader[] SmallFuncHeaders { get; set; }

        public HbcRegExpTableEntry[] RegExpTable { get; set; }

        public HbcCjsModuleTableEntry[] CjsModuleTable { get; set; }

        /// <summary>
        /// The Array Buffer, which contains all constant array data.
        /// </summary>
        public HbcDataBuffer ArrayBuffer { get; set; }

        /// <summary>
        /// The Object Key Buffer, which contains the keys of all constant objects.
        /// </summary>
        public HbcDataBuffer ObjectKeyBuffer { get; set; }

        /// <summary>
        /// The Object Value Buffer, which contains the values of all constant objects.
        /// </summary>
        public HbcDataBuffer ObjectValueBuffer { get; set; }

        public byte[] RegExpStorage { get; set; }

        /// <summary>
        /// Contains all function instructions. Use the function headers (SmallFuncHeaders) to find which function correlates to which instructions.
        /// </summary>
        public byte[] Instructions { get; set; }

        /// <summary>
        /// The offset of the Instructions table in the binary.
        /// </summary>
        public uint InstructionOffset { get; set; }

        /// <summary>
        /// The types of the strings in the string table.
        /// </summary>
        public StringKind[] StringKinds { get; set; }

        /// <summary>
        /// The parsed string table. Index = string index (i.e. from an operand, etc.), Value = string at that index.
        /// </summary>
        public string[] StringTable { get; private set; }

        /// <summary>
        /// Creates a new bytecode file (probably to be writen out). Does not parse anything.
        /// </summary>
        public HbcFile() { }

        /// <summary>
        /// Parses an entire Hermes bytecode file from the given reader.
        /// </summary>
        public HbcFile(HbcReader reader) {
            JObject def = ResourceManager.LoadJsonObject("BytecodeFileFormat");
            Header = HbcEncodedItem.Decode<HbcHeader>(reader, (JObject)def["Header"]);

            if (Header.Magic != HbcHeader.HBC_MAGIC_HEADER) {
                throw new Exception("invalid magic header: not a Hermes bytecode file");
            }

            reader.Align();

            SmallFuncHeaders = new HbcSmallFuncHeader[Header.FunctionCount];
            for (uint i = 0; i < Header.FunctionCount; i++) {
                HbcSmallFuncHeader header = HbcEncodedItem.Decode<HbcSmallFuncHeader>(reader, (JObject)def["SmallFuncHeader"]);
                header.DeclarationFile = this;
                header.FunctionId = i;
                if (header.Flags.HasFlag(HbcFuncHeaderFlags.Overflowed)) {
                    long currentPos = reader.BaseStream.Position;
                    reader.BaseStream.Position = (header.InfoOffset << 16) | header.Offset;

                    header.Large = HbcEncodedItem.Decode<HbcFuncHeader>(reader, (JObject)def["FuncHeader"]);
                    header.Large.DeclarationFile = this;
                    header.Large.FunctionId = i;

                    reader.BaseStream.Position = currentPos;
                }
                SmallFuncHeaders[i] = header;
            }

            reader.Align();

            Console.WriteLine(Header.StringCount);
            Console.WriteLine(Header.StringKindCount);
            Console.WriteLine(Header.IdentifierCount);

            for (uint i = 0; i < Header.StringKindCount; i++) {
                uint stringKind = reader.ReadUInt32();
                Console.WriteLine(Convert.ToString(stringKind, 2));
            }

            reader.Align();

            for (uint i = 0; i < Header.IdentifierCount; i++) {
                uint identifier = reader.ReadUInt32();
                Console.WriteLine(Convert.ToString(identifier, 2));
            }
            reader.Align();

            HbcSmallStringTableEntry[] smallStringTable = new HbcSmallStringTableEntry[Header.StringCount];
            for (int i = 0; i < Header.StringCount; i++) {
                smallStringTable[i] = HbcEncodedItem.Decode<HbcSmallStringTableEntry>(reader, (JObject)def["SmallStringTableEntry"]);
            }

            reader.Align();

            HbcOverflowStringTableEntry[] overflowStringTable = new HbcOverflowStringTableEntry[Header.OverflowStringCount];
            for (int i = 0; i < Header.OverflowStringCount; i++) {
                overflowStringTable[i] = HbcEncodedItem.Decode<HbcOverflowStringTableEntry>(reader, (JObject)def["OverflowStringTableEntry"]);
            }

            reader.Align();

            byte[] stringStorage = reader.ReadBytes((int)Header.StringStorageSize);
            reader.Align();

            ArrayBuffer = new HbcDataBuffer(reader.ReadBytes((int)Header.ArrayBufferSize));
            reader.Align();

            ObjectKeyBuffer = new HbcDataBuffer(reader.ReadBytes((int)Header.ObjKeyBufferSize));
            reader.Align();

            ObjectValueBuffer = new HbcDataBuffer(reader.ReadBytes((int)Header.ObjValueBufferSize));
            reader.Align();

            RegExpTable = new HbcRegExpTableEntry[Header.RegExpCount];
            for (int i = 0; i < Header.RegExpCount; i++) {
                HbcRegExpTableEntry entry = HbcEncodedItem.Decode<HbcRegExpTableEntry>(reader, (JObject)def["RegExpTableEntry"]);
                RegExpTable[i] = entry;
            }

            reader.Align();

            RegExpStorage = reader.ReadBytes((int)Header.RegExpStorageSize);
            reader.Align();

            CjsModuleTable = new HbcCjsModuleTableEntry[Header.CjsModuleCount];
            for (int i = 0; i < Header.CjsModuleCount; i++) {
                HbcCjsModuleTableEntry entry = HbcEncodedItem.Decode<HbcCjsModuleTableEntry>(reader, (JObject)def["CjsModuleTableEntry"]);
                CjsModuleTable[i] = entry;
            }

            reader.Align();

            InstructionOffset = (uint)reader.BaseStream.Position;
            Instructions = new byte[reader.BaseStream.Length - reader.BaseStream.Position];
            reader.BaseStream.Read(Instructions, 0, Instructions.Length);

            CreateStringTable(stringStorage, smallStringTable, overflowStringTable);

            BytecodeFormat = ResourceManager.ReadEmbeddedResource<HbcBytecodeFormat>($"Bytecode{Header.Version}");
        }

        /// <summary>
        /// Writes the Hermes bytecode file and serializes it to a byte array.
        /// </summary>
        public byte[] Write() {
            JObject def = ResourceManager.LoadJsonObject("BytecodeFileFormat");

            using MemoryStream ms = new MemoryStream();
            using HbcWriter writer = new HbcWriter(ms);

            // write the initial header -- some of these values get overwritten when we rewrite the header as the last step
            HbcEncodedItem.Encode(writer, (JObject)def["Header"], Header);
            writer.Align();
            for (uint i = 0; i < Header.FunctionCount; i++) {
                HbcEncodedItem.Encode(writer, (JObject)def["SmallFuncHeader"], SmallFuncHeaders[i]);
                // TODO: large functions
            }
            writer.Align();

            // write string kind count
            foreach (StringKind kind in StringKinds) {

            }

            // write identifier count

            // write small string table

            // write overflow string table

            // write string storage

            // write array buffer
            ArrayBuffer.WriteAll(writer);
            writer.Align();

            // write object key buffer
            ObjectKeyBuffer.WriteAll(writer);
            writer.Align();

            // write object value buffer
            ObjectValueBuffer.WriteAll(writer);
            writer.Align();

            // write regexp table

            // write cjs modules

            // write instructions
            InstructionOffset = (uint) ms.Position;
            writer.Write(Instructions);

            // re-write the header with the final values after writing the rest of the stream
            Header.FileLength = (uint)ms.Position;
            ms.Position = 0;
            HbcEncodedItem.Encode(writer, (JObject)def["Header"], Header);

            ms.Position = Header.FileLength;
            return ms.ToArray();
        }

        /// <summary>
        /// Creates a parsed string table from the raw string storage data.
        /// </summary>
        private void CreateStringTable(byte[] stringStorage, HbcSmallStringTableEntry[] smallStringTable, HbcOverflowStringTableEntry[] overflowStringTable) {
            const uint MAX_STRING_LENGTH = 0xFF;

            StringTable = new string[smallStringTable.Length];
            for (uint i = 0; i < smallStringTable.Length; i++) {
                HbcSmallStringTableEntry entry = smallStringTable[(int)i];

                uint offset = entry.Offset;
                uint length = entry.Length;
                uint isUTF16 = entry.IsUTF16;

                if (length >= MAX_STRING_LENGTH) {
                    HbcOverflowStringTableEntry overflow = overflowStringTable[offset];
                    offset = overflow.Offset;
                    length = overflow.Length;
                }

                if (isUTF16 == 1) {
                    length *= 2;
                }

                byte[] stringBytes = new byte[length];
                Array.Copy(stringStorage, offset, stringBytes, 0, length);

                string str = isUTF16 switch {
                    1 => string.Concat(stringBytes.Select(b => b.ToString("X2"))),
                    _ => Encoding.UTF8.GetString(stringBytes)
                };
                StringTable[i] = str;
            }
        }
    }
}
