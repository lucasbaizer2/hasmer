using System;
using System.Text;
using Hasmer.Decompiler.AST;

namespace Hasmer {
    public class StringTableEntry {
        public StringKind Kind { get; private set; }
        public string Value { get; private set; }
        public bool IsUTF16 { get; private set; }

        public StringTableEntry(StringKind kind, string value, bool isUTF16) {
            Kind = kind;
            Value = value;
            IsUTF16 = isUTF16;
        }

        public bool IsIdentifier => Kind == StringKind.Identifier;
        public bool IsLiteral => Kind == StringKind.Literal;
        public uint Hash => JenkinsHash.Hash(Encoded, IsUTF16);
        public byte[] Encoded {
            get {
                if (IsUTF16) {
                    return Encoding.Unicode.GetBytes(Value);
                } else {
                    return Encoding.ASCII.GetBytes(Value);
                }
            }
        }
        public string Printable {
            get {
                if (IsIdentifier) {
                    return new Identifier(Value).ToString();
                } else {
                    return StringEscape.Escape(Value);
                }
            }
        }
    }
}
