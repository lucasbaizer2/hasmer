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
        /// Represents the data contained with in the header of the Hasm file (i.e. the initial ".hasm" declaration)./
        /// </summary>
        public HasmHeaderReader Header { get; set; }

        /// <summary>
        /// The file builder instance which creates the final <see cref="HbcFile"/> to be serialized.
        /// </summary>
        public HbcFileBuilder FileBuilder { get; set; }

        /// <summary>
        /// The data assembler instance.
        /// </summary>
        public DataAssembler DataAssembler { get; set; }

        /// <summary>
        /// The function assembler instance.
        /// </summary>
        public FunctionAssembler FunctionAssembler { get; set; }

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

            Header = new HasmHeaderReader(stream);
            Header.Read();

            FileBuilder = new HbcFileBuilder(this);

            DataAssembler = new DataAssembler(stream);
            DataAssembler.Assemble();

            FunctionAssembler = new FunctionAssembler(this, stream);
            FunctionAssembler.Assemble();

            HbcFile hbcFile = FileBuilder.Build();
            return hbcFile.Write();
        }
    }
}
