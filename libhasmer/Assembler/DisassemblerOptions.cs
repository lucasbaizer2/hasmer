using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler {
    /// <summary>
    /// Represents the options to be used when disassembling.
    /// </summary>
    public struct DisassemblerOptions {
        /// <summary>
        /// If true, preserve the original instructions and do not abstract out variant instructions.
        /// Additionally, instructions which refer to identifier hashes will have the hash operand omitted.
        /// <br />
        /// If false, instructions will be converted to their variants.
        /// <br />
        /// The default value for this is false.
        /// See <see cref="HbcAbstractInstructionDefinition"/> for more information about variant instructions and abstraction.
        /// </summary>
        public bool IsExact { get; set; }

        /// <summary>
        /// If true, comments will be added to every instruction and function header
        /// describing verbose information about the data as it was originally described in the bytecode file.
        /// </summary>
        public bool IsVerbose { get; set; }
    }
}
