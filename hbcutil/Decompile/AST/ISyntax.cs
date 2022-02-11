using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Decompile.AST {
    public interface ISyntax {
        public void Write(SourceCodeBuilder builder);
    }
}
