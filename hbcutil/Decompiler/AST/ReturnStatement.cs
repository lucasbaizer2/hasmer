using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Decompiler.AST {
    public class ReturnStatement : ISyntax {
        public ISyntax Argument { get; set; }

        public void Write(SourceCodeBuilder builder) {
            builder.Write("return ");
            Argument.Write(builder);
        }
    }
}
