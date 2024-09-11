using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Hasmer {
    /// <summary>
    /// Represents a parsed Hermes bytecode file.
    /// </summary>
    public class HbcFile {
        const uint MAX_STRING_LENGTH = 0xFF;

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
        /// The parsed string table. Index = string index (i.e. from an operand, etc.), Value = string at that index.
        /// </summary>
        public StringTableEntry[] StringTable { get; set; }

        /// <summary>
        /// Creates a new bytecode file (probably to be writen out). Does not parse anything.
        /// </summary>
        public HbcFile() { }

        /// <summary>
        /// Parses an entire Hermes bytecode file from the given reader.
        /// </summary>
        public HbcFile(HbcReader reader) {
            Header = new HbcHeader {
                Magic = reader.ReadUInt64()
            };
            if (Header.Magic != HbcHeader.HBC_MAGIC_HEADER) {
                throw new Exception("invalid magic header: not a Hermes bytecode file");
            }

            Header.Version = reader.ReadUInt32();
            Header.SourceHash = new byte[20];
            reader.Read(Header.SourceHash);
            Header.FileLength = reader.ReadUInt32();
            Header.GlobalCodeIndex = reader.ReadUInt32();
            Header.FunctionCount = reader.ReadUInt32();
            Header.StringKindCount = reader.ReadUInt32();
            Header.IdentifierCount = reader.ReadUInt32();
            Header.StringCount = reader.ReadUInt32();
            Header.OverflowStringCount = reader.ReadUInt32();
            Header.StringStorageSize = reader.ReadUInt32();
            if (Header.Version >= 87) {
                Header.BigIntCount = reader.ReadUInt32();
                Header.BigIntStorageSize = reader.ReadUInt32();
            }
            Header.RegExpCount = reader.ReadUInt32();
            Header.RegExpStorageSize = reader.ReadUInt32();
            Header.ArrayBufferSize = reader.ReadUInt32();
            Header.ObjKeyBufferSize = reader.ReadUInt32();
            Header.ObjValueBufferSize = reader.ReadUInt32();
            Header.CjsModuleOffset = reader.ReadUInt32();
            Header.CjsModuleCount = reader.ReadUInt32();
            Header.DebugInfoOffset = reader.ReadUInt32();
            Header.Options = (HbcBytecodeOptions)reader.ReadByte();
            Header.Padding = new byte[31];
            reader.Read(Header.Padding);

            reader.Align();

            SmallFuncHeaders = new HbcSmallFuncHeader[Header.FunctionCount];
            for (uint i = 0; i < Header.FunctionCount; i++) {
                HbcSmallFuncHeader header = new HbcSmallFuncHeader {
                    Offset = reader.ReadBits(25),
                    ParamCount = reader.ReadBits(7),
                    BytecodeSizeInBytes = reader.ReadBits(15),
                    FunctionName = reader.ReadBits(17),
                    InfoOffset = reader.ReadBits(25),
                    FrameSize = reader.ReadBits(7),
                    EnvironmentSize = reader.ReadBits(8),
                    HighestReadCacheIndex = reader.ReadBits(8),
                    HighestWriteCacheIndex = reader.ReadBits(8),
                    Flags = (HbcFuncHeaderFlags)reader.ReadByte(),
                    DeclarationFile = this,
                    FunctionId = i
                };
                if (header.Flags.HasFlag(HbcFuncHeaderFlags.Overflowed)) {
                    long currentPos = reader.BaseStream.Position;
                    reader.BaseStream.Position = (header.InfoOffset << 16) | header.Offset;

                    header.Large = new HbcFuncHeader {
                        Offset = reader.ReadUInt32(),
                        ParamCount = reader.ReadUInt32(),
                        BytecodeSizeInBytes = reader.ReadUInt32(),
                        FunctionName = reader.ReadUInt32(),
                        InfoOffset = reader.ReadUInt32(),
                        FrameSize = reader.ReadUInt32(),
                        EnvironmentSize = reader.ReadUInt32(),
                        HighestReadCacheIndex = reader.ReadByte(),
                        HighestWriteCacheIndex = reader.ReadByte(),
                        Flags = (HbcFuncHeaderFlags)reader.ReadByte(),
                        DeclarationFile = this,
                        FunctionId = i
                    };
                    reader.BaseStream.Position = currentPos;
                }
                SmallFuncHeaders[i] = header;
            }
            reader.Align();

            StringKindEntry[] stringKinds = new StringKindEntry[Header.StringKindCount];
            for (uint i = 0; i < Header.StringKindCount; i++) {
                stringKinds[i] = new StringKindEntry(reader.ReadUInt32());
            }
            reader.Align();

            uint[] identifierHashes = new uint[Header.IdentifierCount];
            for (uint i = 0; i < Header.IdentifierCount; i++) {
                identifierHashes[i] = reader.ReadUInt32();
            }
            reader.Align();

            HbcSmallStringTableEntry[] smallStringTable = new HbcSmallStringTableEntry[Header.StringCount];
            for (int i = 0; i < Header.StringCount; i++) {
                smallStringTable[i] = new HbcSmallStringTableEntry {
                    IsUTF16 = reader.ReadBit(),
                    Offset = reader.ReadBits(23),
                    Length = reader.ReadBits(8),
                };
            }
            reader.Align();

            HbcOverflowStringTableEntry[] overflowStringTable = new HbcOverflowStringTableEntry[Header.OverflowStringCount];
            for (int i = 0; i < Header.OverflowStringCount; i++) {
                overflowStringTable[i] = new HbcOverflowStringTableEntry {
                    Offset = reader.ReadUInt32(),
                    Length = reader.ReadUInt32(),
                };
            }
            reader.Align();

            byte[] stringStorage = reader.ReadBytes((int)Header.StringStorageSize);
            reader.Align();

            if (Header.Version >= 87) {
                // TODO: parse the BigIntTable
                throw new Exception("BigIntTable not yet implemented");
            }

            ArrayBuffer = new HbcDataBuffer(reader.ReadBytes((int)Header.ArrayBufferSize));
            reader.Align();

            ObjectKeyBuffer = new HbcDataBuffer(reader.ReadBytes((int)Header.ObjKeyBufferSize));
            reader.Align();

            ObjectValueBuffer = new HbcDataBuffer(reader.ReadBytes((int)Header.ObjValueBufferSize));
            reader.Align();

            RegExpTable = new HbcRegExpTableEntry[Header.RegExpCount];
            for (int i = 0; i < Header.RegExpCount; i++) {
                RegExpTable[i] = new HbcRegExpTableEntry {
                    Offset = reader.ReadUInt32(),
                    Length = reader.ReadUInt32(),
                };
            }

            reader.Align();

            RegExpStorage = reader.ReadBytes((int)Header.RegExpStorageSize);
            reader.Align();

            CjsModuleTable = new HbcCjsModuleTableEntry[Header.CjsModuleCount];
            for (int i = 0; i < Header.CjsModuleCount; i++) {
                CjsModuleTable[i] = new HbcCjsModuleTableEntry {
                    First = reader.ReadUInt32(),
                    Second = reader.ReadUInt32(),
                };
            }
            reader.Align();

            InstructionOffset = (uint)reader.BaseStream.Position;
            Instructions = new byte[reader.BaseStream.Length - reader.BaseStream.Position];
            reader.BaseStream.Read(Instructions, 0, Instructions.Length);

            CreateStringTable(stringStorage, smallStringTable, overflowStringTable, stringKinds, identifierHashes);

            BytecodeFormat = ResourceManager.ReadEmbeddedResource<HbcBytecodeFormat>($"Bytecode{Header.Version}");
        }

        /// <summary>
        /// Writes the Hermes bytecode file and serializes it to a byte array.
        /// </summary>
        public byte[] Write() {
            Header.StringCount = (uint)StringTable.Length;
            Header.StringKindCount = 0;
            Header.IdentifierCount = 0;
            Header.OverflowStringCount = 0;
            Header.StringStorageSize = 0;
            Header.ArrayBufferSize = (uint)ArrayBuffer.Buffer.Length;
            Header.ObjKeyBufferSize = (uint)ObjectKeyBuffer.Buffer.Length;
            Header.ObjValueBufferSize = (uint)ObjectValueBuffer.Buffer.Length;
            Header.FunctionCount = (uint)SmallFuncHeaders.Length;

            JObject def = ResourceManager.LoadJsonObject("BytecodeFileFormat");

            using MemoryStream ms = new MemoryStream();
            using HbcWriter writer = new HbcWriter(ms);

            // write the initial header -- these values get overwritten when we rewrite the header as the last step
            HbcEncodedItem.Encode(writer, (JObject)def["Header"], Header);
            writer.Align();

            foreach (HbcSmallFuncHeader header in SmallFuncHeaders) {
                HbcEncodedItem.Encode(writer, (JObject)def["SmallFuncHeader"], header);
                // TODO: large functions
            }
            writer.Align();

            // write string kinds
            if (StringTable.Length > 0) {
                StringKind currentKind = StringTable[0].Kind;
                uint currentCount = 1;
                for (uint i = 1; i < StringTable.Length; i++) {
                    StringTableEntry entry = StringTable[i];
                    if (entry.Kind == currentKind) {
                        currentCount++;
                    } else {
                        writer.Write(new StringKindEntry(currentKind, currentCount).Entry);

                        Header.StringKindCount++;
                        currentKind = entry.Kind;
                        currentCount = 1;
                    }
                }

                // make sure to write the last run of string kinds
                writer.Write(new StringKindEntry(currentKind, currentCount).Entry);
            }

            // write identifier hashes
            for (uint i = 0; i < StringTable.Length; i++) {
                StringTableEntry entry = StringTable[i];
                if (entry.IsIdentifier) {
                    writer.Write(entry.Hash);
                    Header.IdentifierCount++;
                }
            }
            writer.Align();

            // encode all strings, indexable by their ID
            byte[][] encodedStrings = new byte[StringTable.Length][];
            uint[] encodedStringsAreUTF16 = new uint[StringTable.Length];
            for (uint i = 0; i < StringTable.Length; i++) {
                StringTableEntry ste = StringTable[i];
                byte[] encoded = ste.Encoded;
                encodedStrings[i] = encoded;
                encodedStringsAreUTF16[i] = ste.IsUTF16 ? 1u : 0u;

                if (encoded.Length >= MAX_STRING_LENGTH) {
                    Header.OverflowStringCount++;
                }
                Header.StringStorageSize += (uint)encoded.Length;
            }

            // construct the contiguous string storage, small string table, and large 
            byte[] stringStorage = new byte[Header.StringStorageSize];
            HbcSmallStringTableEntry[] smallStrings = new HbcSmallStringTableEntry[StringTable.Length];
            HbcOverflowStringTableEntry[] overflowStrings = new HbcOverflowStringTableEntry[Header.OverflowStringCount];
            uint stringStorageOffset = 0;
            uint overflowOffset = 0;
            for (uint i = 0; i < StringTable.Length; i++) {
                byte[] encoded = encodedStrings[i];
                uint isUTF16 = encodedStringsAreUTF16[i];
                uint encodingDivisor = isUTF16 == 0u ? 1u : 2u;

                HbcSmallStringTableEntry sste;
                if (encoded.Length >= MAX_STRING_LENGTH) {
                    sste = new HbcSmallStringTableEntry {
                        IsUTF16 = isUTF16,
                        Offset = overflowOffset,
                        Length = MAX_STRING_LENGTH,
                    };

                    overflowStrings[overflowOffset] = new HbcOverflowStringTableEntry {
                        Offset = stringStorageOffset,
                        Length = (uint)encoded.Length / encodingDivisor,
                    };

                    overflowOffset++;
                } else {
                    sste = new HbcSmallStringTableEntry {
                        IsUTF16 = isUTF16,
                        Offset = stringStorageOffset,
                        Length = (uint)encoded.Length / encodingDivisor,
                    };
                }
                smallStrings[i] = sste;

                Array.Copy(encoded, 0, stringStorage, stringStorageOffset, encoded.Length);
                stringStorageOffset += (uint)encoded.Length / encodingDivisor;
            }

            Console.WriteLine($" wSmallStrings @ {ms.Position}");

            // write small string table
            foreach (HbcSmallStringTableEntry entry in smallStrings) {
                HbcEncodedItem.Encode(writer, (JObject)def["SmallStringTableEntry"], entry);
            }
            writer.Align();

            // write overflow string table
            foreach (HbcOverflowStringTableEntry entry in overflowStrings) {
                HbcEncodedItem.Encode(writer, (JObject)def["OverflowStringTableEntry"], entry);
            }
            writer.Align();

            // write string storage
            writer.Write(stringStorage);
            writer.Align();

            // TODO: read BigIntTable

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
            InstructionOffset = (uint)ms.Position;
            writer.Write(Instructions);

            // re-write the header with the final values after writing the rest of the stream
            Header.FileLength = (uint)ms.Position;
            ms.Position = 0;
            HbcEncodedItem.Encode(writer, (JObject)def["Header"], Header);

            Console.WriteLine($"Wrote HBC!\n  IdentifierCount = {Header.IdentifierCount}\n  StringCount = {Header.StringCount}");

            ms.Position = Header.FileLength;
            return ms.ToArray();
        }

        public StringTableEntry GetStringTableEntry(int index) {
            if (index < 0 || index >= StringTable.Length) {
                throw new Exception("out of bounds string index: " + index);
            }
            return StringTable[index];
        }

        /// <summary>
        /// Creates a parsed string table from the raw string storage data.
        /// </summary>
        private void CreateStringTable(byte[] stringStorage, HbcSmallStringTableEntry[] smallStringTable, HbcOverflowStringTableEntry[] overflowStringTable, StringKindEntry[] stringKinds, uint[] identifierHashes) {
            Console.WriteLine($"stringStorage.Length = {stringStorage.Length}");

            StringKind[] kindLookup = new StringKind[smallStringTable.Length];
            int k = 0;
            foreach (StringKindEntry entry in stringKinds) {
                for (int i = 0; i < entry.Count; i++, k++) {
                    kindLookup[k] = entry.Kind;
                }
            }

            StringTable = new StringTableEntry[smallStringTable.Length];
            int identIdx = 0;
            for (uint i = 0; i < smallStringTable.Length; i++) {
                HbcSmallStringTableEntry entry = smallStringTable[(int)i];

                uint offset = entry.Offset;
                uint length = entry.Length;
                bool isUTF16 = entry.IsUTF16 != 0;

                Console.WriteLine($"  offset = {offset}, length = {length}, isUTF16 = {isUTF16}");

                if (length >= MAX_STRING_LENGTH) {
                    HbcOverflowStringTableEntry overflow = overflowStringTable[offset];
                    offset = overflow.Offset;
                    length = overflow.Length;
                    Console.WriteLine($"    overflow; offset = {offset}, length = {length}");
                }

                if (isUTF16) {
                    length *= 2;
                    Console.WriteLine($"    isUTF16; length = {length}");
                }

                byte[] stringBytes = new byte[length];
                Array.Copy(stringStorage, offset, stringBytes, 0, length);

                StringKind kind = kindLookup[i];
                Encoding enc = isUTF16 ? Encoding.Unicode : Encoding.ASCII;
                string str = enc.GetString(stringBytes);

                StringTableEntry stEntry = new StringTableEntry(kind, str, isUTF16);
                if (stEntry.IsIdentifier) {
                    uint hash = identifierHashes[identIdx++];
                    if (stEntry.Hash != hash) {
                        Console.WriteLine($"Warning: identifier '{stEntry.Value}' has invalid hash; expecting {hash} but calculated {stEntry.Hash}");
                    }
                }
                StringTable[i] = stEntry;
            }
        }
    }
}
