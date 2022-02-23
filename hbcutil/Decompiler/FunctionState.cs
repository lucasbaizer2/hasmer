using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HbcUtil.Decompiler.AST;

namespace HbcUtil.Decompiler {
    /// <summary>
    /// Represents the state of a function as it is decompiled.
    /// </summary>
    public class FunctionState {
        /// <summary>
        /// Represents the contents of each register as a JavaScript syntax object.
        /// </summary>
        public ISyntax[] Registers { get; set; }
        /// <summary>
        /// Represents the current local variables as an array. Index = register, Value = variable name.
        /// </summary>
        public string[] Variables { get; set; }

        /// <summary>
        /// Creates a new FunctionState, allocating a set amount of registers (frame size).
        /// </summary>
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
