using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer {
    /// <summary>
    /// Represents a string that was too large to be in the normal string table.
    /// </summary>
    public class HbcOverflowStringTableEntry : HbcEncodedItem {
        public uint Offset { get; set; }
        public uint Length { get; set; }
    }
}
