using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visitors for instructions that load constant values.
    /// </summary>
    [VisitorCollection]
    public class LoadConstantOperations {
        /// <summary>
        /// Clears the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstEmpty(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = null;
        }

        /// <summary>
        /// Loads the JavaScript identifier "undefined" into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstUndefined(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Identifier("undefined");
        }

        /// <summary>
        /// Loads the JavaScript literal null into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstNull(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(null));
        }

        /// <summary>
        /// Loads the JavaScript literal boolean `true` into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstTrue(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(true));
        }

        /// <summary>
        /// Loads the JavaScript literal boolean `false` into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstFalse(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(false));
        }

        /// <summary>
        /// Loads the JavaScript literal number 0 into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstZero(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(0));
        }

        /// <summary>
        /// Loads a given constant string value into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstString(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            string str = context.Instruction.Operands[1].GetResolvedValue<string>(context.Source);
            context.State.Registers[register] = new Literal(new PrimitiveValue(str));
        }

        /// <summary>
        /// Loads a given constant numerical value into the specified register, coerced as type T.
        /// </summary>
        private static void LoadConstNumerical<T>(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            T value = context.Instruction.Operands[1].GetValue<T>();
            context.State.Registers[register] = new Literal(new PrimitiveValue(value));
        }

        /// <summary>
        /// Loads a constant unsigned byte into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstUInt8(DecompilerContext context) {
            LoadConstNumerical<byte>(context);
        }

        /// <summary>
        /// Loads a constant unsigned unsigned 4-byte integer into the specified register.
        /// </summary>
        [Visitor]
        public static void LoadConstInt(DecompilerContext context) {
            LoadConstNumerical<uint>(context);
        }
    }
}
