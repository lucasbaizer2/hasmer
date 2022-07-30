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

            ProgramDefinition root = new ProgramDefinition();

            // create a list containing the IDs of all functions
            List<uint> rootFunctions = new List<uint>();
            for (uint i = 0; i < Source.SmallFuncHeaders.Length; i++) {
                rootFunctions.Add(i);
            }

            foreach (HbcSmallFuncHeader header in Source.SmallFuncHeaders) {
                List<HbcInstruction> insns = header.Disassemble();
                foreach (HbcInstruction insn in insns) {
                    HbcInstructionDefinition def = Source.BytecodeFormat.Definitions[insn.Opcode];
                    if (def.Name == "CreateClosure") {
                        uint closureId = insn.Operands[2].GetValue<uint>();
                        rootFunctions.Remove(closureId); // remove functions defined as closures from the root functions list
                    }
                }
            }

            // decompile each non-closure function at the root
            // closures will be expanded recursively through each function that creates the closure
            foreach (uint funcId in rootFunctions) {
                FunctionDecompiler decompiler = new FunctionDecompiler(this, Source.SmallFuncHeaders[funcId]);
                SyntaxNode ast = decompiler.CreateAST(null);

                if (funcId == 0) { // write the global function as just its body withou the header
                    FunctionDeclaration globalFunc = (FunctionDeclaration)ast;
                    root.Body = root.Body.Concat(globalFunc.Body.Body).ToList();
                } else {
                    root.Body.Add(ast);
                }
            }

            if (preserveAst) {
                return JsonConvert.SerializeObject(root, Formatting.Indented);
            } else {
                SourceCodeBuilder builder = new SourceCodeBuilder("    ");
                root.Write(builder);
                return builder.ToString();
            }
        }
    }
}
