﻿using System;
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
        public ISyntax[] Storage { get; set; }

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
            Storage = new ISyntax[registers];
            State = state;
        }

        /// <summary>
        /// Gets the syntax located at the given register.
        /// If a call expression is located at a register which is being overriden with a new value,
        /// the call expression is immediately added to the source tree, and then the register is replaced with the argument.
        /// </summary>
        public ISyntax this[uint index] {
            get => Storage[index];
            set {
                if (value != null) {
                    ISyntax previous = Storage[index];
                    if (previous is CallExpression) {
                        State.Context.Block.Body.Add(previous);
                    }
                }
                Storage[index] = value;
            }
        }

        public ISyntax this[int index] {
            get => this[(uint)index];
            set => this[(uint)index] = value;
        }
    }

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
        /// Creates a new FunctionState, allocating a set amount of registers (frame size).
        /// </summary>
        public FunctionState(DecompilerContext context, uint registers) {
            Context = context;
            Registers = new RegisterTracker(this, registers);
            Variables = new string[registers];
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