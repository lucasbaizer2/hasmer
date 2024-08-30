using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer {
    /// <summary>
    /// Used for determining whether a string is an identifier or a string literal.
    /// </summary>
    [Flags]
    public enum StringKind {
        /// <summary>
        /// The string is a string literal, e.g. a string in quotes: `var s = "Hello, World!"`.
        /// "Hello, World!" would be encoded as a string literal, i.e. this flag.
        /// </summary>
        Literal = 0 << StringKindEntry.CountBits,
        /// <summary>
        /// The string is an identifier, e.g. the name of a function.
        /// </summary>
        Identifier = 1 << StringKindEntry.CountBits
    }

    public struct StringKindEntry {
        public const int CountBits = 31;
        const uint MaxCount = (1u << CountBits) - 1;

        public uint Entry { get; set; }

        public StringKindEntry(uint entry) {
            Entry = entry;
        }

        public StringKindEntry(StringKind kind, uint count) {
            Entry = (uint)kind | (count & MaxCount);
        }

        public StringKind Kind => (StringKind)(Entry & ~MaxCount);

        public int Count => (int)(Entry & MaxCount);
    }
}
