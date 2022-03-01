using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Visits operations that create new objects/arrays/etc.
    /// </summary>
    [VisitorCollection]
    public class CreationOperations {
        /// <summary>
        /// Creates a new JavaScript object (i.e. "{}").
        /// </summary>
        [Visitor]
        public static void NewObject(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            context.State.Variables[resultRegister] = "obj" + resultRegister;
            context.State.Registers[resultRegister] = new Identifier(context.State.Variables[resultRegister]);

            context.Block.Body.Add(new AssignmentExpression {
                Left = new Identifier(context.State.Variables[resultRegister]),
                Right = new ObjectExpression(),
                Operator = "="
            });
        }

        /// <summary>
        /// Creates a new JavaScript array with a predefined length, i.e. "new Array(length)".
        /// </summary>
        [Visitor]
        public static void NewArray(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            ushort arrayLength = context.Instruction.Operands[1].GetValue<ushort>();
            context.State.Variables[resultRegister] = "arr" + resultRegister;
            context.State.Registers[resultRegister] = new Identifier(context.State.Variables[resultRegister]);

            context.Block.Body.Add(new AssignmentExpression {
                Left = context.State.Registers[resultRegister],
                Right = new CallExpression {
                    Callee = new MemberExpression {
                        Object = new Identifier("global") {
                            IsRedundant = context.Decompiler.Options.OmitExplicitGlobal
                        },
                        Property = new Identifier("Array")
                    },
                    Arguments = new List<SyntaxNode>() {
                        new Literal(new PrimitiveValue(arrayLength))
                    },
                    IsCalleeConstructor = true
                },
                Operator = "="
            });
        }

        /// <summary>
        /// Creates a new JavaScript array with predefined values from the array buffer.
        /// </summary>
        [Visitor]
        public static void NewArrayWithBuffer(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            ushort itemsCount = context.Instruction.Operands[2].GetValue<ushort>();
            uint arrayBufferIndex = context.Instruction.Operands[3].GetValue<uint>();

            ArrayExpression arr = new ArrayExpression();
            context.State.Variables[resultRegister] = "arr" + resultRegister;
            context.State.Registers[resultRegister] = new Identifier(context.State.Variables[resultRegister]);

            PrimitiveValue[] items = context.Decompiler.DataDisassembler.GetElementSeries(context.Decompiler.DataDisassembler.ArrayBuffer, arrayBufferIndex, itemsCount);
            arr.Elements = items.Select(item => new Literal(item)).Cast<SyntaxNode>().ToList();

            context.Block.Body.Add(new AssignmentExpression {
                Left = context.State.Registers[resultRegister],
                Right = arr,
                Operator = "="
            });
        }

        /// <summary>
        /// Creates a new closure.
        /// In the future, this should not just be an identifier to a closure, but should actually decompile the closure function.
        /// </summary>
        [Visitor]
        public static void CreateClosure(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            uint closureId = context.Instruction.Operands[2].GetValue<uint>();

            FunctionDecompiler closureDecompiler = new FunctionDecompiler(context.Decompiler, context.Source.SmallFuncHeaders[closureId].GetAssemblerHeader());
            SyntaxNode closureAST = closureDecompiler.CreateAST(context);

            context.State.Registers[resultRegister] = closureAST;
            // context.State.Registers[resultRegister] = new Identifier($"$closure${closureId}");
        }

        /// <summary>
        /// Creates a new "this" instance from a prototype.
        /// </summary>
        [Visitor]
        public static void CreateThis(DecompilerContext context) {
            byte resultRegister = context.Instruction.Operands[0].GetValue<byte>();
            byte prototypeRegister = context.Instruction.Operands[1].GetValue<byte>();

            // CreateThis is a VM construct more or less, so we can just consider the prototype definition as the "this" instance
            context.State.Registers[resultRegister] = context.State.Registers[prototypeRegister];
        }
    }
}
