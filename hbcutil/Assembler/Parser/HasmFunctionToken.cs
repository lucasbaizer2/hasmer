using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public class HasmFunctionToken : HasmToken {
        public string FunctionName { get; set; }
        public List<HasmToken> Body { get; set; }
        public uint Id { get; set; }
        public uint ParameterCoumt { get; set; }
        public uint RegisterCount { get; set; }
        public uint Symbols { get; set; }

        public HasmFunctionToken(HasmStringStreamState state) : base(state) { }
    }
}
