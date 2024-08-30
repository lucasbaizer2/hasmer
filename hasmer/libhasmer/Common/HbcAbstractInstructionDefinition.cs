using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hasmer {
    /// <summary>
    /// Represents the abstract form of variant instructions.
    /// Variant instructions are instructions which perform the same action, but can have differently sized operands.
    /// By abstracting these instructions to all have one name, the assembler can optimize the size of the operands.
    /// Thus, programmers do not have to figure out the proper sizes when they write Hasm code.
    /// <br /> <br />
    /// Passing the "--exact" flag to the hasmer disassmbler will ignore abstract definitions,
    /// and instead emit the exact instruction.
    /// </summary>
    public class HbcAbstractInstructionDefinition {
        /// <summary>
        /// The abstract name that can be used to represent any of the <see cref="Variants"/>.
        /// <br />
        /// For example, the instructions "JStrictNotEqual" and "JStrictNotEqualLong"
        /// will have an abstract name of simply "JStrictNotEqual".
        /// <br />
        /// The assembler will decide which to use based on the operands at assemble time.
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// The opcodes of each variant that can be defined by this abstract definition.
        /// </summary>
        [JsonProperty]
        public List<uint> VariantOpcodes { get; set; }
    }
}
