using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public interface ISyntax {
        public void Write(SourceCodeBuilder builder);
    }
}
