using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Analysis {
    public class StaticAnalyzerState {
        public Dictionary<Identifier, SyntaxNode> Registers { get; set; }
        public List<string> Variables { get; set;  }

        public StaticAnalyzerState() {
            Registers = new Dictionary<Identifier, SyntaxNode>();
            Variables = new List<string>();
        }

        public string GetVariableName(string name) {
            // name = "get" + char.ToUpper(name[0]) + name.Substring(1);
            name = "_" + name;
            string originalName = name;
            int index = 2;
            while (Variables.Contains(name)) {
                name = originalName + (index++);
            }
            Variables.Add(name);
            return name;
        }
    }
}
