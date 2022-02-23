using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HbcUtil {
    /// <summary>
    /// Represents a SmallFuncHeader, which is just a FuncHeader but also includes a pointer to the full function.
    /// This is the default function information implementation in bytecode files, but if the function's contents (i.e. bytecode, register count, etc)
    /// cannot be included within the bounds of a SmallFuncHeader, this object includes a pointer to the full header as well.
    /// </summary>
    public class HbcSmallFuncHeader : HbcFuncHeader {
        public HbcFuncHeader Large { get; set; }

        public override HbcFuncHeader GetAssemblerHeader() {
            return Large ?? this;
        }
    }
}
