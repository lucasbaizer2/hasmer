namespace Hasmer {
    /// <summary>
    /// Represents the header of a Hermes bytecode file.
    /// </summary>
    public class HbcHeader : HbcEncodedItem {
        /// <summary>
        /// The constant magic header that is present at the start of all Hermes bytecode files.
        /// </summary>
        public const ulong HBC_MAGIC_HEADER = 0x1F1903C103BC1FC6;

        /// <summary>
        /// The magic header of the file. This should be equal to <see cref="HBC_MAGIC_HEADER"/>.
        /// </summary>
        public ulong Magic { get; set; }

        /// <summary>
        /// The Hermes bytecode version of the file.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// A hex string representing the SHA1 hash of the original JavaScript source code that was compiled into Hermes bytecode.
        /// </summary>
        public byte[] SourceHash { get; set; }

        /// <summary>
        /// The total length in bytes of the bytecode file.
        /// </summary>
        public uint FileLength { get; set; }

        /// <summary>
        /// The index in the functions table of the global function.
        /// </summary>
        public uint GlobalCodeIndex { get; set; }

        /// <summary>
        /// The total amount of functions declared in the binary.
        /// </summary>
        public uint FunctionCount { get; set; }

        public uint StringKindCount { get; set; }

        public uint IdentifierCount { get; set; }

        /// <summary>
        /// The amount of strings defined in the strings table.
        /// </summary>
        public uint StringCount { get; set; }

        /// <summary>
        /// The amount of strings who have overflows (i.e. too long to be treated normally).
        /// </summary>
        public uint OverflowStringCount { get; set; }

        /// <summary>
        /// The total size in bytes of the raw string storage buffer.
        /// </summary>
        public uint StringStorageSize { get; set; }

        public uint? BigIntCount { get; set; }

        public uint? BigIntStorageSize { get; set; }

        public uint RegExpCount { get; set; }

        public uint RegExpStorageSize { get; set; }

        /// <summary>
        /// The total size in bytes of the array buffer, where constant array data is stored.
        /// </summary>
        public uint ArrayBufferSize { get; set; }

        /// <summary>
        /// The total size in bytes of the object key buffer, where constant object keys are stored.
        /// </summary>
        public uint ObjKeyBufferSize { get; set; }

        /// <summary>
        /// The total size in bytes of the object galue buffer, where constant object values are stored.
        /// </summary>
        public uint ObjValueBufferSize { get; set; }

        // Replaced CjsModuleOffset in HBC version 78.
        public uint SegmentID { get; set; }

        public uint CjsModuleOffset { get; set; }

        public uint CjsModuleCount { get; set; }

        // Added in HBC version 84.
        public uint FunctionSourceCount { get; set; }

        /// <summary>
        /// The offset in the binary of debug information, if present.
        /// </summary>
        public uint DebugInfoOffset { get; set; }

        /// <summary>
        /// The global options of the bytecode file, which are used by the Hermes runtime.
        /// </summary>
        public HbcBytecodeOptions Options { get; set; }

        /// <summary>
        /// 31 bytes of arbitrary padding.
        /// </summary>
        public byte[] Padding { get; set; }
    }

    /// <summary>
    /// The global options of the bytecode file, which are used by the Hermes runtime.
    /// </summary>
    public enum HbcBytecodeOptions : byte {
        StaticBuiltins = 0x1,
        CjsModulesStaticallyResolved = 0x2,
        HasAsync = 0x4
    }
}
