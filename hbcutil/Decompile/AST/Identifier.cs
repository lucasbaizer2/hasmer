using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Decompile.AST {
    public class Identifier : ISyntax {
        public string Name { get; set; }

        public Identifier(string name) {
            Name = name;
        }

        public void Write(SourceCodeBuilder builder) {
            builder.Write(Name);
        }
    }
}
