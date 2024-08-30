using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler {
    /// <summary>
    /// Represents the state of a function as it is decompiled.
    /// </summary>
    public class FunctionState {
        /// <summary>
        /// The decompiler context that uses this FunctionState.
        /// </summary>
        public DecompilerContext Context { get; set; }

        /// <summary>
        /// Represents the contents of each register as a JavaScript syntax object.
        /// </summary>
        public RegisterTracker Registers { get; set; }

        /// <summary>
        /// Represents the current local variables as an array. Index = register, Value = variable name.
        /// </summary>
        public string[] Variables { get; set; }

        /// <summary>
        /// Represents mappings between registers and the call expression whose result was stored into that register.
        /// <br />
        /// The index of the array represents the register that the call expression was written into.
        /// The value at a given index of the array represents the index of the call expression token in the current <see cref="DecompilerContext.Block"/>.
        /// </summary>
        public int[] CallExpressions { get; set; }

        /// <summary>
        /// Creates a new FunctionState, allocating a set amount of registers (frame size).
        /// </summary>
        public FunctionState(DecompilerContext context, uint registers) {
            Context = context;
            Registers = new RegisterTracker(this, registers);
            Variables = new string[registers];
            CallExpressions = new int[registers];

            for (uint i = 0; i < CallExpressions.Length; i++) {
                CallExpressions[i] = -1;
            }
        }

        public void DebugPrint() {
            for (int i = 0; i < Registers.Length; i++) {
                Console.Write($"Register {i}: ");
                if (Registers[i] == null) {
                    Console.WriteLine("empty");
                } else {
                    SourceCodeBuilder builder = new SourceCodeBuilder("    ");
                    builder.Write($"[{Registers[i].GetType().Name}] ");
                    Registers[i].Write(builder);
                    Console.WriteLine(builder.ToString());
                }
                Console.WriteLine($"Variable {i}: {Variables[i]}");
            }
            Console.WriteLine("----------------------------------");
        }
    }
}
