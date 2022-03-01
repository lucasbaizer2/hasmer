using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer {
    /// <summary>
    /// Represents a string in the string raw buffer. Used for creating the string table.
    /// </summary>
    public class HbcSmallStringTableEntry : HbcEncodedItem {
        public uint IsUTF16 { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }
    }
}
