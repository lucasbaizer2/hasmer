using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hasmer.Decompiler.AST {
    /// <summary>
    /// Represents a token in the JavaScript syntax tree.
    /// </summary>
    public abstract class SyntaxNode {
        internal SyntaxNode Previous { get; set; }
        internal SyntaxNode Next { get; set; }
        internal SyntaxNode Parent { get; set; }
        public bool Replaced { get; private set; }

        public void ReplaceWith(SyntaxNode node) {
            if (Replaced) {
                throw new Exception("node has already been replaced");
            }
            if (Parent is BlockStatement block) {
                block.Body[block.Body.IndexOf(this)] = node;
            } else if (Parent is AssignmentExpression assn) {
                if (this == assn.Left) {
                    assn.Left = node;
                } else if (this == assn.Right) {
                    assn.Right = node;
                } else {
                    throw new Exception();
                }
            } else if (Parent is BinaryExpression binary) {
                if (this == binary.Left) {
                    binary.Left = node;
                } else if (this == binary.Right) {
                    binary.Right = node;
                } else {
                    throw new Exception();
                }
            } else if (Parent is MemberExpression member) {
                if (this == member.Object) {
                    member.Object = node;
                } else if (this == member.Property) {
                    member.Property = node;
                } else {
                    throw new Exception();
                }
            } else if (Parent is CallExpression call) {
                if (this == call.Callee) {
                    call.Callee = node;
                } else {
                    for (int i = 0; i < call.Arguments.Count; i++) {
                        if (this == call.Arguments[i]) {
                            call.Arguments[i] = node;
                            return;
                        }
                    }
                    throw new Exception();
                }
            } else if (Parent is ReturnStatement ret) {
                if (this == ret.Argument) {
                    ret.Argument = node;
                } else {
                    throw new Exception();
                }
            } else if (Parent is UnaryExpression unary) {
                if (this == unary.Argument) {
                    unary.Argument = node;
                } else {
                    throw new Exception();
                }
            } else {
                throw new Exception(Parent?.GetType()?.Name ?? "Parent == null");
            }
            node.Parent = Parent;
            node.Previous = Previous;
            node.Next = Next;

            if (Previous != null) {
                Previous.Next = node;
            }
            if (Next != null) {
                Next.Previous = node;
            }

            Replaced = true;
        }

#pragma warning disable IDE0051 // Remove unused private members
        /// <summary>
        /// Gets the type of the token (the class name of the implementation) as information when JSON serializing the token (used for debug).
        /// </summary>
        [JsonProperty("TokenType")]
        private string JsonTokenType => GetType().Name;
#pragma warning restore IDE0051 // Remove unused private members

        public void Write(SourceCodeBuilder builder) {
            if (Replaced) {
                throw new Exception("cannot write replaced node");
            }
            WriteDirect(builder);
        }

        /// <summary>
        /// Writes the token as a string to the given <see cref="SourceCodeBuilder"/>.
        /// </summary>
        public abstract void WriteDirect(SourceCodeBuilder builder);

        public T Cast<T>() where T : SyntaxNode {
            return (T)this;
        }
    }
}
