using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil {
    public class HbcSmallStringTableEntry : HbcEncodedItem {
        public uint IsUTF16 { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }
    }
}
