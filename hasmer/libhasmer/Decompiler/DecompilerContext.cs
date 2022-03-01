using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler {
    /// <summary>
    /// Represents the context of a function as it is being decompiled.
    /// </summary>
    public class DecompilerContext {
        /// <summary>
        /// The decompiler for the Hermes bytecode file.
        /// </summary>
        public HbcDecompiler Decompiler { get; set; }

        /// <summary>
        /// The bytecode file containing the function being decompiled.
        /// </summary>
        public HbcFile Source => Decompiler.Source;

        /// <summary>
        /// The instructions in the current code block being decompiled.
        /// </summary>
        public List<HbcInstruction> Instructions { get; set; }

        /// <summary>
        /// The state of the function's registers and other information in the current code bock.
        /// </summary>
        public FunctionState State { get; set; }

        /// <summary>
        /// The index of the instruction currently being analyzed in the <see cref="Instructions"/> list.
        /// </summary>
        public int CurrentInstructionIndex { get; set; }

        /// <summary>
        /// The current instruction being analyzed.
        /// </summary>
        public HbcInstruction Instruction => Instructions[CurrentInstructionIndex];

        /// <summary>
        /// The current source tree code block that is being analyzed.
        /// </summary>
        public BlockStatement Block { get; set; }

        /// <summary>
        /// The decompiler context of the parent function,
        /// i.e. if this function is a closure, this will be the decompiler context of the callee.
        /// A parent of null means that the function is at the root.
        /// </summary>
        public DecompilerContext Parent { get; set; }

        /// <summary>
        /// The header of the function that is being decompiled.
        /// </summary>
        public HbcFuncHeader Function { get; set; }

        /// <summary>
        /// Gets the parent context at the given depth.
        /// The current context is depth 0, the parent context is depth 1, etc.
        /// If the parent at the given depth is beyond that of a root function (i.e. a function whose parent is null),
        /// then an exception is thrown. Thus, this method can never return null.
        /// </summary>
        public DecompilerContext GetDeepParent(int depth) {
            if (depth < 0) {
                throw new IndexOutOfRangeException("depth < 0");
            }

            DecompilerContext parent = this;
            for (int i = 0; i < depth; i++) {
                parent = parent.Parent;
                if (parent == null) {
                    throw new IndexOutOfRangeException($"depth exceeds stack: {depth}");
                }
            }

            return parent;
        }

        /// <summary>
        /// Creates a semi-deep copy of the current decompiler context,
        /// copying the function state and instructions so that the original context's state is not modified by the copy.
        /// </summary>
        public DecompilerContext DeepCopy() {
            DecompilerContext copy = new DecompilerContext {
                Decompiler = Decompiler,
                Instructions = new List<HbcInstruction>(Instructions),
                Block = Block,
                CurrentInstructionIndex = CurrentInstructionIndex,
                Parent = Parent,
                Function = Function
            };
            copy.State = new FunctionState(copy, (uint)State.Registers.Length);

            Array.Copy(State.Registers.Storage, copy.State.Registers.Storage, State.Registers.Length);
            Array.Copy(State.Variables, copy.State.Variables, State.Variables.Length);
            Array.Copy(State.CallExpressions, copy.State.CallExpressions, State.CallExpressions.Length);

            return copy;
        }
    }
}
