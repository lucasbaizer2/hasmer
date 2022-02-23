using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Decompiler.AST;

namespace HbcUtil.Decompiler {
    public class FunctionState {
        public ISyntax[] Registers { get; set; }
        public string[] Variables { get; set; }

        public FunctionState(uint registers) {
            Registers = new ISyntax[registers];
            Variables = new string[registers];
        }

        public void DebugPrint() {
            for (int i = 0; i < Registers.Length; i++) {
                Console.Write($"Register {i}: ");
                if (Registers[i] == null) {
                    Console.WriteLine("empty");
                } else {
                    SourceCodeBuilder builder = new SourceCodeBuilder("    ");
                    Registers[i].Write(builder);
                    Console.WriteLine(builder.ToString());
                }
                Console.WriteLine($"Variable {i}: {Variables[i]}");
            }
        }
    }
}
