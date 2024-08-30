using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler {
    /// <summary>
    /// Represents the registers of a function, and keeps track of the operations being performed on them.
    /// </summary>
    public class RegisterTracker {
        /// <summary>
        /// Represents the contents of each register as a JavaScript syntax object.
        /// </summary>
        public SyntaxNode[] Storage { get; set; }

        /// <summary>
        /// Represents the amount of times the value of each register has been referenced while the register retains that value.
        /// <br />
        /// Each time a register is referenced in an operand, the instruction visitor calls <see cref="MarkUsage(uint)"/>.
        /// When the value of the register is changed, the usage is reset to 0.
        /// </summary>
        public int[] RegisterUsages { get; set; }

        /// <summary>
        /// The amount of registers contained in the <see cref="Storage"/> array.
        /// </summary>
        public int Length => Storage.Length;

        /// <summary>
        /// The state of the function that the registers tracked by the RegisterTracker relate to.
        /// </summary>
        private FunctionState State;

        /// <summary>
        /// Creates a new RegisterTracker given an amount of registers to store and keep track of.
        /// </summary>
        public RegisterTracker(FunctionState state, uint registers) {
            Storage = new SyntaxNode[registers];
            RegisterUsages = new int[registers];
            State = state;
        }

        /// <summary>
        /// Marks a usage of a register.
        /// Each time a register is used as a value in an operand,
        /// this method should be invoked with the operand as the parameter.
        /// <br />
        /// See <see cref="RegisterUsages"/>.
        /// </summary>
        public void MarkUsage(uint register) {
            RegisterUsages[register]++;
        }

        /// <summary>
        /// Marks a usage of multiple registers.
        /// See <see cref="MarkUsage(uint)"/>.
        /// </summary>
        public void MarkUsages(params uint[] registers) {
            foreach (uint reg in registers) {
                MarkUsage(reg);
            }
        }

        /// <summary>
        /// Gets the syntax located at the given register.
        /// If a call expression is located at a register which is being overriden with a new value,
        /// the call expression is immediately added to the source tree, and then the register is replaced with the argument.
        /// </summary>
        public SyntaxNode this[uint register] {
            get {
                SyntaxNode value = Storage[register];
                if (value is CallExpression && State.CallExpressions[register] != -1) {
                    // using the result of a call expression -- clear it from the call expressions table
                    State.CallExpressions[register] = -1;
                }
                return value;
            }
            set {
                if (value != null) {
                    SyntaxNode previous = Storage[register];
                    if (previous is CallExpression && State.CallExpressions[register] != -1) {
                        // if we overwrite the result of a call expression, but never actually used the value, that means the call was never decompiled
                        // so we explicitly write the decompilation here

                        State.Context.Block.Body[State.CallExpressions[register]] = previous; // overwrite the placeholder empty token with the call expression
                        State.CallExpressions[register] = -1; // once we've written the call expression, mark that call expression as written
                    }
                }

                RegisterUsages[register] = 0;
                Storage[register] = value;
            }
        }

        public SyntaxNode this[int index] {
            get => this[(uint)index];
            set => this[(uint)index] = value;
        }
    }
}
