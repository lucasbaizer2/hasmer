namespace HbcUtil {
    /// <summary>
    /// Represents the header of a Hermes bytecode file.
    /// </summary>
    public class HbcHeader : HbcEncodedItem {
        /// <summary>
        /// The magic header of the file. This is always equal to 0x1F1903C103BC1FC6.
        /// </summary>
        public ulong Magic { get; set; }
        /// <summary>
        /// The Hermes bytecode version of the file.
        /// </summary>
        public uint Version { get; set; }
        /// <summary>
        /// The hex string representing the SHA1 hash of the file.
        /// </summary>
        public byte[] SourceHash { get; set; }
        /// <summary>
        /// The total length in bytes of the file.
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
        /// The amount of strings declared.
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
        public uint CjsModuleOffset { get; set; }
        public uint CjsModuleCount { get; set; }
        /// <summary>
        /// The offset in the binary of debug information, if present.
        /// </summary>
        public uint DebugInfoOffset { get; set; }
        public byte Option { get; set; }
        public byte[] Padding { get; set; }
    }
}
