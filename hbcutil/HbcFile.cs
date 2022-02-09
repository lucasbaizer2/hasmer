using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace HbcUtil {
    public class HbcFile {
        public HbcHeader Header { get; private set; }
        public HbcBytecodeFormat BytecodeFormat { get; private set; }
        public List<HbcSmallFuncHeader> SmallFuncHeaders { get; private set; }
        public List<HbcSmallStringTableEntry> SmallStringTable { get; private set; }
        public List<HbcOverflowStringTableEntry> OverflowStringTable { get; private set; }
        public List<HbcRegExpTableEntry> RegExpTable { get; private set; }
        public List<HbcCjsModuleTableEntry> CjsModuleTable { get; private set; }
        public byte[] StringStorage { get; private set; }
        public byte[] ArrayBuffer { get; private set; }
        public byte[] ObjKeyBuffer { get; private set; }
        public byte[] ObjValueBuffer { get; private set; }
        public byte[] RegExpStorage { get; private set; }
        public byte[] Instructions { get; private set; }
        public uint InstructionOffset { get; private set; }

        public HbcFile(HbcReader reader) {
            JObject def = ResourceManager.LoadJsonObject("BytecodeFileFormat");
            Header = HbcEncodedItem.Decode<HbcHeader>(reader, (JObject)def["Header"]);

            if (Header.Magic != 0x1F1903C103BC1FC6) {
                throw new Exception("invalid magic header: not a Hermes bytecode file");
            }

            reader.Align();

            SmallFuncHeaders = new List<HbcSmallFuncHeader>((int)Header.FunctionCount);
            for (int i = 0; i < Header.FunctionCount; i++) {
                HbcSmallFuncHeader header = HbcEncodedItem.Decode<HbcSmallFuncHeader>(reader, (JObject)def["SmallFuncHeader"]);
                header.DeclarationFile = this;
                if (((header.Flags >> 5) & 1) == 1) {
                    long currentPos = reader.BaseStream.Position;
                    reader.BaseStream.Position = (header.InfoOffset << 16) | header.Offset;

                    header.Large = HbcEncodedItem.Decode<HbcFuncHeader>(reader, (JObject)def["FuncHeader"]);
                    reader.BaseStream.Position = currentPos;
                }
                SmallFuncHeaders.Add(header);
            }

            reader.Align();
            reader.BaseStream.Position += Header.StringKindCount * 4;
            reader.Align();
            reader.BaseStream.Position += Header.IdentifierCount * 4;
            reader.Align();

            SmallStringTable = new List<HbcSmallStringTableEntry>((int)Header.StringCount);
            for (int i = 0; i < Header.StringCount; i++) {
                HbcSmallStringTableEntry entry = HbcEncodedItem.Decode<HbcSmallStringTableEntry>(reader, (JObject)def["SmallStringTableEntry"]);
                SmallStringTable.Add(entry);
            }

            reader.Align();

            OverflowStringTable = new List<HbcOverflowStringTableEntry>((int)Header.OverflowStringCount);
            for (int i = 0; i < Header.OverflowStringCount; i++) {
                HbcOverflowStringTableEntry entry = HbcEncodedItem.Decode<HbcOverflowStringTableEntry>(reader, (JObject)def["OverflowStringTableEntry"]);
                OverflowStringTable.Add(entry);
            }

            reader.Align();

            def["StringStorage"][1] = Header.StringStorageSize;
            StringStorage = (byte[])HbcEncodedItem.ParseFromDefinition(reader, def["StringStorage"]);
            reader.Align();

            def["ArrayBuffer"][1] = Header.ArrayBufferSize;
            ArrayBuffer = (byte[])HbcEncodedItem.ParseFromDefinition(reader, def["ArrayBuffer"]);
            reader.Align();

            def["ObjKeyBuffer"][1] = Header.ObjKeyBufferSize;
            ObjKeyBuffer = (byte[])HbcEncodedItem.ParseFromDefinition(reader, def["ObjKeyBuffer"]);
            reader.Align();

            def["ObjValueBuffer"][1] = Header.ObjValueBufferSize;
            ObjValueBuffer = (byte[])HbcEncodedItem.ParseFromDefinition(reader, def["ObjValueBuffer"]);
            reader.Align();

            RegExpTable = new List<HbcRegExpTableEntry>((int)Header.RegExpCount);
            for (int i = 0; i < Header.RegExpCount; i++) {
                HbcRegExpTableEntry entry = HbcEncodedItem.Decode<HbcRegExpTableEntry>(reader, (JObject)def["RegExpTableEntry"]);
                RegExpTable.Add(entry);
            }

            reader.Align();

            def["RegExpStorage"][1] = Header.RegExpStorageSize;
            RegExpStorage = (byte[])HbcEncodedItem.ParseFromDefinition(reader, def["RegExpStorage"]);
            reader.Align();

            CjsModuleTable = new List<HbcCjsModuleTableEntry>((int)Header.RegExpCount);
            for (int i = 0; i < Header.CjsModuleCount; i++) {
                HbcCjsModuleTableEntry entry = HbcEncodedItem.Decode<HbcCjsModuleTableEntry>(reader, (JObject)def["CjsModuleTableEntry"]);
                CjsModuleTable.Add(entry);
            }

            reader.Align();

            InstructionOffset = (uint)reader.BaseStream.Position;
            Instructions = new byte[reader.BaseStream.Length - reader.BaseStream.Position];
            reader.BaseStream.Read(Instructions, 0, Instructions.Length);

            BytecodeFormat = ResourceManager.ReadEmbeddedResource<HbcBytecodeFormat>($"Bytecode{Header.Version}");
        }
    }
}
