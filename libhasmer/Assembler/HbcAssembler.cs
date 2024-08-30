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
        /// The data assembler instance.
        /// </summary>
        public DataAssembler DataAssembler { get; set; }

        /// <summary>
        /// The function assembler instance.
        /// </summary>
        public FunctionAssembler FunctionAssembler { get; set; }

        /// <summary>
        /// The Hermes bytecode file being assembled.
        /// </summary>
        public HbcFile File { get; set; }

        public bool IsExact { get; set; }

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
            Console.WriteLine("Parsing Hasm file...");
            HasmTokenizer tokenizer = new HasmTokenizer();
            tokenizer.TokenizeProgram(Source);

            // List<HasmToken> tokens = null;

            // IsExact = tokenStream.State.IsExact;
            // File = new HbcFile {
            //     Header = new HbcHeader {
            //         Magic = HbcHeader.HBC_MAGIC_HEADER,
            //         Version = tokenStream.State.BytecodeFormat.Version,
            //         SourceHash = new byte[20],
            //         GlobalCodeIndex = 0,
            //         Padding = new byte[31],
            //     },
            //     BytecodeFormat = tokenStream.State.BytecodeFormat,
            // };

            // Console.WriteLine("Assembling data...");
            // DataAssembler = new DataAssembler(tokens);
            // DataAssembler.Assemble();

            // Console.WriteLine("Assembling functions...");
            // FunctionAssembler = new FunctionAssembler(this, tokens);
            // FunctionAssembler.Assemble();

            // Console.WriteLine("Assembling Hermes bytecode file...");
            // File.StringTable = DataAssembler.StringTable.ToArray();
            // File.ArrayBuffer = new HbcDataBuffer(DataAssembler.ArrayBuffer.RawBuffer.ToArray());
            // File.ObjectKeyBuffer = new HbcDataBuffer(DataAssembler.ObjectKeyBuffer.RawBuffer.ToArray());
            // File.ObjectValueBuffer = new HbcDataBuffer(DataAssembler.ObjectValueBuffer.RawBuffer.ToArray());

            // return File.Write();

            return new byte[0];
        }
    }
}
