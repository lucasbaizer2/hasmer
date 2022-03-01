using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Hasmer.Assembler.Parser;
using Hasmer.Assembler.Visitor;

namespace Hasmer.Assembler {
    /// <summary>
    /// Represents a Hasm assembler, which assembles into Hermes bytecode.
    /// </summary>
    public class HbcAssembler {
        /// <summary>
        /// The Hasm assembly to disassemble.
        /// </summary>
        private string Source;

        /// <summary>
        /// Creates a new HbcAssembler, given the raw source of the Hasm assembly.
        /// </summary>
        public HbcAssembler(string source) {
            Source = source;
        }

        /// <summary>
        /// Assembles the given Hasm assembly into a Hermes bytecode file, serialized into a byte array.
        /// </summary>
        public byte[] Assemble() {
            HasmTokenStream stream = new HasmTokenStream(Source);
            HbcBuilder writer = new HbcBuilder(stream);
            return writer.Write();
        }
    }
}
