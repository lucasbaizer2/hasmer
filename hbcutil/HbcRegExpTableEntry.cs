﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil {
    public class HbcRegExpTableEntry : HbcEncodedItem {
        public uint Offset { get; set; }
        public uint Length { get; set; }
    }
}
