namespace HbcUtil {
    public class HbcHeader : HbcEncodedItem {
        public ulong Magic { get; set; }
        public uint Version { get; set; }
        public byte[] SourceHash { get; set; }
        public uint FileLength { get; set; }
        public uint GlobalCodeIndex { get; set; }
        public uint FunctionCount { get; set; }
        public uint StringKindCount { get; set; }
        public uint IdentifierCount { get; set; }
        public uint StringCount { get; set; }
        public uint OverflowStringCount { get; set; }
        public uint StringStorageSize { get; set; }
        public uint RegExpCount { get; set; }
        public uint RegExpStorageSize { get; set; }
        public uint ArrayBufferSize { get; set; }
        public uint ObjKeyBufferSize { get; set; }
        public uint ObjValueBufferSize { get; set; }
        public uint CjsModuleOffset { get; set; }
        public uint CjsModuleCount { get; set; }
        public uint DebugInfoOffset { get; set; }
        public byte Option { get; set; }
        public byte[] Padding { get; set; }
    }
}
