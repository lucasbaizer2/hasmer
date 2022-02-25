using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler.Visitor {
    /// <summary>
    /// Represents a function being built by the assembler.
    /// </summary>
    public class HbcFunctionBuilder {
        /// <summary>
        /// The declared ID of the function.
        /// </summary>
        public uint FunctionId { get; set; }

        /// <summary>
        /// The declared amount of parameters the function takes.
        /// </summary>
        public uint ParamCount { get; set; }

        /// <summary>
        /// The declared name of the function.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// The amount of registers that the function has.
        /// </summary>
        public uint FrameSize { get; set; }

        /// <summary>
        /// The amount of symbols that the function has.
        /// </summary>
        public uint EnvironmentSize { get; set; }

        /// <summary>
        /// The flags of the function.
        /// </summary>
        public HbcFuncHeaderFlags Flags { get; set; }

        /// <summary>
        /// The instructions contained in the body of the function.
        /// </summary>
        public List<HbcInstruction> Instructions { get; set; }
    }
}
