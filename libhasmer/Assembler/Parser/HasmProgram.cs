using System.Collections.Generic;

namespace Hasmer.Assembler.Parser {
    public class HasmProgram {
        public HasmHeader Header { get; set; }
        public List<HasmDataDeclaration> Data { get; set; }
        public List<HasmFunctionToken> Functions { get; set; }
    }
}
