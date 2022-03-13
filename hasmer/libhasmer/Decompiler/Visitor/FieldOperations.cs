using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits instructions that gets/sets fields in an object.
    /// </summary>
    [VisitorCollection]
    public class FieldOperations {
        /// <summary>
        /// Gets the result of a constructor, either the 'this' instance or a returned object.
        /// For the purpose of the decompiler, it's irrelevant what the result is,
        /// so the constructor result register (from a previous call) is copied.
        /// </summary>
        [Visitor]
        public static void SelectObject(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte constructorResultRegister = context.Instruction.Operands[2].GetValue<byte>();

            context.State.Registers.MarkUsage(constructorResultRegister);

            context.State.Registers[resultRegister] = context.State.Registers[constructorResultRegister];
        }

        /// <summary>
        /// Declares a global variable and initializes it to "undefined".
        /// This ignores the <see cref="DecompilerOptions.OmitExplicitGlobal"/> option,
        /// as it always explictly references the "global" field.
        /// This is to show that the variable is not local, and is rather explicitly global.
        /// </summary>
        [Visitor]
        public static void DeclareGlobalVar(DecompilerContext context) {
            string varName = context.Instruction.Operands[0].GetResolvedValue<string>(context.Source);
            context.Block.Body.Add(new AssignmentExpression {
                Left = new MemberExpression {
                    Object = new Identifier("global"),
                    Property = new Identifier(varName)
                },
                Right = new Identifier("undefined"),
                Operator = "="
            });
        }

        /// <summary>
        /// Loads either the "this" value if inside a function, or "global" if inside the global function.
        /// </summary>
        [Visitor]
        public static void LoadThisNS(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            if (context.Function.FunctionId == 0) { // global function
                context.State.Registers[register] = new Identifier("global") {
                    IsRedundant = context.Decompiler.Options.OmitExplicitGlobal
                };
            } else {
                context.State.Registers[register] = new Identifier("this");
            }
        }

        /// <summary>
        /// Returns an identifier representing the global object.
        /// </summary>
        [Visitor]
        public static void GetGlobalObject(DecompilerContext context) {
            byte register = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Registers[register] = new Identifier("global") {
                IsRedundant = context.Decompiler.Options.OmitExplicitGlobal
            };
        }

        /// <summary>
        /// Gets a field reference by its name (i.e. by identifier).
        /// </summary>
        public static void CommonGetById(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();
            string identifier = context.Instruction.Operands[3].GetResolvedValue<string>(context.Source);

            context.State.Registers.MarkUsage(sourceRegister);

            context.State.Registers[resultRegister] = new MemberExpression {
                Object = context.State.Registers[sourceRegister],
                Property = new Identifier(identifier)
            };
        }

        [Visitor]
        public static void GetById(DecompilerContext context) => CommonGetById(context);

        [Visitor]
        public static void GetByIdShort(DecompilerContext context) => CommonGetById(context);

        [Visitor]
        public static void GetByIdLong(DecompilerContext context) => CommonGetById(context);

        [Visitor]
        public static void TryGetById(DecompilerContext context) => CommonGetById(context);

        [Visitor]
        public static void TryGetByIdLong(DecompilerContext context) => CommonGetById(context);

        /// <summary>
        /// Sets the value of a field reference by name (i.e. by identifier).
        /// </summary>
        private static void CommonPutById(DecompilerContext context, SyntaxNode property) {
            byte objRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();

            SyntaxNode obj;
            if (context.State.Variables[objRegister] != null) {
                obj = new Identifier(context.State.Variables[objRegister]);
            } else {
                obj = context.State.Registers[objRegister];
            }

            context.State.Registers.MarkUsage(objRegister);
            context.State.Registers.MarkUsage(sourceRegister);

            context.Block.Body.Add(new AssignmentExpression {
                Operator = "=",
                Left = new MemberExpression(false) {
                    Object = obj,
                    Property = property,
                    IsComputed = false
                },
                Right = context.State.Registers[sourceRegister]
            });
        }

        /// <summary>
        /// Sets the value of a field reference by value.
        /// </summary>
        [Visitor]
        public static void PutByVal(DecompilerContext context) {
            byte objRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte valRegister = context.Instruction.Operands[1].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[2].GetValue<byte>();

            SyntaxNode obj;
            if (context.State.Variables[objRegister] != null) {
                obj = new Identifier(context.State.Variables[objRegister]);
            } else {
                obj = context.State.Registers[objRegister];
            }

            context.State.Registers.MarkUsage(objRegister);
            context.State.Registers.MarkUsage(valRegister);
            context.State.Registers.MarkUsage(sourceRegister);

            context.Block.Body.Add(new AssignmentExpression {
                Operator = "=",
                Left = new MemberExpression(false) {
                    Object = obj,
                    Property = context.State.Registers[valRegister],
                    IsComputed = true
                },
                Right = context.State.Registers[sourceRegister]
            });
        }

        [Visitor]
        public static void PutById(DecompilerContext context) =>
            CommonPutById(context, new Identifier(context.Source.StringTable[context.Instruction.Operands[3].GetValue<uint>()]));

        [Visitor]
        public static void PutNewOwnById(DecompilerContext context) =>
            CommonPutById(context, new Identifier(context.Source.StringTable[context.Instruction.Operands[2].GetValue<uint>()]));

        [Visitor]
        public static void PutNewOwnByIdShort(DecompilerContext context) =>
            CommonPutById(context, new Identifier(context.Source.StringTable[context.Instruction.Operands[2].GetValue<uint>()]));

        [Visitor]
        public static void PutOwnByIndex(DecompilerContext context) =>
            CommonPutById(context, new Literal(new PrimitiveValue(context.Instruction.Operands[2].GetValue<uint>())));

        /// <summary>
        /// Gets a field reference by value (i.e. the value contained within an identifier).
        /// <br />
        /// In comparison to <see cref="GetById(DecompilerContext)"/>,
        /// GetByVal represents "obj[x]" whereas GetById represents "obj.x".
        /// </summary>
        [Visitor]
        public static void GetByVal(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();
            uint identifierRegister = context.Instruction.Operands[2].GetValue<uint>();

            context.State.Registers.MarkUsage(sourceRegister);
            context.State.Registers.MarkUsage(identifierRegister);

            context.State.Registers[resultRegister] = new MemberExpression(false) {
                Object = context.State.Registers[sourceRegister],
                Property = context.State.Registers[identifierRegister],
                IsComputed = true
            };
        }

        /// <summary>
        /// Deletes a field from an object, with the field indicated by a value in a register.
        /// </summary>
        [Visitor]
        public static void DelByVal(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();
            uint identifierRegister = context.Instruction.Operands[2].GetValue<uint>();

            context.State.Registers.MarkUsage(sourceRegister);
            context.State.Registers.MarkUsage(identifierRegister);

            context.Block.Body.Add(new UnaryExpression {
                Operator = "delete",
                Argument = new MemberExpression(false) {
                    Object = context.State.Registers[sourceRegister],
                    Property = context.State.Registers[identifierRegister],
                    IsComputed = true
                }
            });
            context.State.Registers[resultRegister] = new Literal(new PrimitiveValue(true)); // for the purposes of decompilation, the delete operator always returns true
        }

        /// <summary>
        /// Coerces a given register into a number value and places the number into the given register.
        /// </summary>
        [Visitor]
        public static void ToNumber(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte sourceRegister = context.Instruction.Operands[1].GetValue<byte>();

            context.State.Registers.MarkUsage(sourceRegister);

            context.State.Registers[resultRegister] = new CallExpression {
                Callee = new Identifier("Number"),
                Arguments = new List<SyntaxNode> {
                    context.State.Registers[sourceRegister]
                }
            };
        }
    }
}
