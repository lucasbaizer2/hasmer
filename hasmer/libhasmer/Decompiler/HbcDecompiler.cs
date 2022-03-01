using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Hasmer.Assembler;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler {
    /// <summary>
    /// Represents a decompiler of a Hermes bytecode file, used for approximating the original JavaScript source.
    /// </summary>
    public class HbcDecompiler {
        /// <summary>
        /// The options to be used when decompiling.
        /// </summary>
        public DecompilerOptions Options { get; set; }

        /// <summary>
        /// The Hermes bytecode file used for decompilation.
        /// </summary>
        public HbcFile Source { get; set; }

        /// <summary>
        /// The disassembler used for parsing data from the bytecode file's data buffers.
        /// </summary>
        public DataDisassembler DataDisassembler { get; set; }

        /// <summary>
        /// Creates a new HbcDecompiler given the bytecode file to decompile and the options to be used for decompiling.
        /// </summary>
        public HbcDecompiler(HbcFile source, DecompilerOptions options) {
            Source = source;
            Options = options;
            DataDisassembler = new DataDisassembler(source);
        }

        /// <summary>
        /// Decompiles the bytecode file into either a JSON AST or JavaScript source code.
        /// </summary>
        public string Decompile(bool preserveAst) {
            DataDisassembler.DisassembleData();
            FunctionDecompiler decompiler = new FunctionDecompiler(this, Source.SmallFuncHeaders[197]);
            ISyntax ast = decompiler.CreateAST(null);
            if (preserveAst) {
                return JsonConvert.SerializeObject(ast, Formatting.Indented);
            } else {
                SourceCodeBuilder builder = new SourceCodeBuilder("    ");
                ast.Write(builder);
                return builder.ToString();
            }
        }
    }
}
