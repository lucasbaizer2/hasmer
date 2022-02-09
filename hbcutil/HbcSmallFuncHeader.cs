using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HbcUtil {
    public class HbcSmallFuncHeader : HbcFuncHeader {
        public HbcFuncHeader Large { get; set; }
    }
}
