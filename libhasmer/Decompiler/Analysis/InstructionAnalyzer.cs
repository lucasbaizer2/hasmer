using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Analysis {
    public class InstructionAnalyzer {
        public List<SyntaxNode> Body { get; set; }
        public StaticAnalyzerState State { get; set; }
        public DecompilerOptions Options { get; set; }

        private Identifier FindIdentifierReference(SyntaxNode node, Identifier ident) {
            if (node == null) {
                return null;
            }
            if (node is Identifier otherIdent) {
                if (otherIdent.Name == ident.Name) {
                    return otherIdent;
                }
                return null;
            }
            if (node is ArrayExpression array) {
                return array.Elements.Select(element => FindIdentifierReference(element, ident)).FirstOrDefault(x => x != null);
            }
            if (node is AssignmentExpression assignment) {
                Identifier right = FindIdentifierReference(assignment.Right, ident);
                Identifier left;
                if (assignment.Left is Identifier) {
                    left = null;
                } else if (assignment.Left is MemberExpression asnexp && asnexp.GetUltimateProperty() is Identifier asnident && asnident.Name == ident.Name) {
                    left = null;
                } else {
                    left = FindIdentifierReference(assignment.Left, ident);
                }
                return right ?? left;
            }
            if (node is BinaryExpression expr) {
                return FindIdentifierReference(expr.Left, ident) ?? FindIdentifierReference(expr.Right, ident);
            }
            if (node is CallExpression call) {
                return FindIdentifierReference(call.Callee, ident)
                    ?? call.Arguments.Select(arg => FindIdentifierReference(arg, ident)).FirstOrDefault(x => x != null);
            }
            if (node is IfStatement ifs) {
                return FindIdentifierReference(ifs.Test, ident);
            }
            if (node is MemberExpression member) {
                return FindIdentifierReference(member.Object, ident) ?? FindIdentifierReference(member.Property, ident);
            }
            if (node is ReturnStatement ret) {
                return FindIdentifierReference(ret.Argument, ident);
            }
            if (node is UnaryExpression unary) {
                return FindIdentifierReference(unary.Argument, ident);
            }

            return null;
        }

        private void RefactorVariableName(AssignmentExpression originalAssignment, SyntaxNode startNode, string newVariableName = null, Identifier originalIdentifier = null) {
            Identifier identifier = originalIdentifier ?? originalAssignment.Left.Cast<Identifier>();

            string variableName;
            if (newVariableName == null) {
                string rootName = null;
                if (originalAssignment.Right is CallExpression call) {
                    if (call.Callee is MemberExpression) {
                        SyntaxNode node = call.Callee;
                        if (node is MemberExpression expr) {
                            node = expr.GetUltimateProperty();
                        }
                        if (node is Identifier ident) {
                            rootName = ident.Name;
                        }
                    } else if (call.Callee is Identifier ident) {
                        rootName = ident.Name;
                    }
                } else if (originalAssignment.Right is ArrayExpression) {
                    rootName = "arr";
                } else if (originalAssignment.Right is ObjectExpression) {
                    rootName = "obj";
                }
                if (rootName == null) {
                    return;
                }
                variableName = State.GetVariableName(rootName);
                originalAssignment.DeclarationKind = "let";
            } else {
                variableName = newVariableName;
            }

            SyntaxNode currentNode = startNode;
            while ((currentNode = currentNode.Next) != null) {
                /*
                if (currentNode is AssignmentExpression assn && assn.Left is Identifier left && left.Name == identifier.Name) {
                    left.ReplaceWith(new Identifier(variableName));
                }
                */
                Identifier reference;
                while ((reference = FindIdentifierReference(currentNode, identifier)) != null) {
                    reference.ReplaceWith(new Identifier(variableName));
                }

                if (currentNode is AssignmentExpression assn && assn.Left is Identifier left && left.Name == identifier.Name) {
                    break;
                }

                if (currentNode is IfStatement ifs) {
                    if (ifs.Consequent is BlockStatement ifc) {
                        RefactorVariableName(originalAssignment, new EmptyExpression {
                            Next = ifc.Body[0]
                        }, variableName, identifier);
                    }
                    if (ifs.Alternate is BlockStatement ifa) {
                        RefactorVariableName(originalAssignment, new EmptyExpression {
                            Next = ifa.Body[0]
                        }, variableName, identifier);
                    }
                }
            }

            if (!identifier.Replaced) {
                identifier.ReplaceWith(new Identifier(variableName));
            }
        }

        private void RemoveUnusedAssignment(AssignmentExpression originalAssignment) {
            Identifier identifier = originalAssignment.Left.Cast<Identifier>();
            SyntaxNode currentNode = originalAssignment;
            while ((currentNode = currentNode.Next) != null) {
                if (FindIdentifierReference(currentNode, identifier) != null) {
                    // break when the identifier is referenced
                    return;
                }
                if (currentNode is AssignmentExpression assn && assn.Operator == "=" && assn.Left is Identifier left && left.Name == identifier.Name) {
                    // if we hit here, that means the identifier was never referenced before reassignment
                    // so we can remove the left side of the assignment expression from the original
                    if (originalAssignment.Right is CallExpression) {
                        originalAssignment.ReplaceWith(originalAssignment.Right);
                    } else {
                        originalAssignment.ReplaceWith(new EmptyExpression());
                    }
                    return;
                }
            }

            // there were no references and we hit the end of the block before any assignment
            // therefore is it unused
            if (originalAssignment.Right is CallExpression) {
                originalAssignment.ReplaceWith(originalAssignment.Right);
            } else {
                originalAssignment.ReplaceWith(new EmptyExpression());
            }
        }

        private bool RefactorConstantAssignments(AssignmentExpression originalAssignment, SyntaxNode startNode) {
            Identifier identifier = originalAssignment.Left.Cast<Identifier>();
            SyntaxNode value = originalAssignment.Right;
            if (value is CallExpression || value is ObjectExpression || value is ArrayExpression) {
                return false;
            }

            bool replacedAny = false;
            SyntaxNode currentNode = startNode;
            while ((currentNode = currentNode.Next) != null) {
                Identifier reference;
                while ((reference = FindIdentifierReference(currentNode, identifier)) != null) {
                    reference.ReplaceWith(value);
                    replacedAny = true;
                }
                if (currentNode is AssignmentExpression assn && assn.Operator == "=" && assn.Left is Identifier left && left.Name == identifier.Name) {
                    break;
                }

                if (currentNode is IfStatement ifs) {
                    if (ifs.Consequent is BlockStatement ifc) {
                        replacedAny |= RefactorConstantAssignments(originalAssignment, new EmptyExpression {
                            Next = ifc.Body[0]
                        });
                    }
                    if (ifs.Alternate is BlockStatement ifa) {
                        replacedAny |= RefactorConstantAssignments(originalAssignment, new EmptyExpression {
                            Next = ifa.Body[0]
                        });
                    }
                }
            }

            if (replacedAny && !originalAssignment.Replaced) {
                originalAssignment.ReplaceWith(new EmptyExpression());
            }

            return replacedAny;
        }

        private void GenerateMetadata(SyntaxNode node) {
            if (node is BlockStatement sub) {
                GenerateBlockMetadata(sub);
            } else if (node is IfStatement ifs) {
                ifs.Test.Parent = node;
                GenerateMetadata(ifs.Test);
                if (ifs.Consequent is BlockStatement ifc) {
                    ifc.Parent = node;
                    GenerateBlockMetadata(ifc);
                }
                if (ifs.Alternate is BlockStatement ifa) {
                    ifa.Parent = node;
                    GenerateBlockMetadata(ifa);
                }
            } else if (node is FunctionDeclaration dec) {
                dec.Body.Parent = node;
                GenerateBlockMetadata(dec.Body);
            } else if (node is AssignmentExpression assn) {
                assn.Left.Parent = node;
                assn.Right.Parent = node;
                GenerateMetadata(assn.Left);
                GenerateMetadata(assn.Right);
            } else if (node is BinaryExpression binary) {
                binary.Left.Parent = node;
                binary.Right.Parent = node;
                GenerateMetadata(binary.Left);
                GenerateMetadata(binary.Right);
            } else if (node is MemberExpression member) {
                member.Object.Parent = node;
                member.Property.Parent = node;
                GenerateMetadata(member.Object);
                GenerateMetadata(member.Property);
            } else if (node is CallExpression call) {
                call.Callee.Parent = node;
                GenerateMetadata(call.Callee);
                foreach (SyntaxNode arg in call.Arguments) {
                    arg.Parent = node;
                    GenerateMetadata(arg);
                }
            } else if (node is ReturnStatement ret) {
                ret.Argument.Parent = node;
                GenerateMetadata(ret.Argument);
            } else if (node is UnaryExpression unary) {
                unary.Argument.Parent = node;
                GenerateMetadata(unary.Argument);
            } else if (node is ArrayExpression array) {
                foreach (SyntaxNode e in array.Elements) {
                    e.Parent = node;
                }
            } else if (node is Identifier || node is Literal || node is RegExpLiteral || node is ObjectExpression) {
                if (node.Parent == null) {
                    throw new Exception("constant without a parent");
                }
            } else if (node is not EmptyExpression) {
                throw new Exception(node?.GetType().Name ?? "node == null");
            }
        }

        private void GenerateBlockMetadata(BlockStatement block) {
            for (int i = 0; i < block.Body.Count; i++) {
                SyntaxNode node = block.Body[i];
                if (i < block.Body.Count - 1) {
                    node.Next = block.Body[i + 1];
                }
                if (i > 0) {
                    node.Previous = block.Body[i - 1];
                }
                node.Parent = block;

                GenerateMetadata(node);
            }
        }

        public void Optimize(BlockStatement block) {
            GenerateBlockMetadata(block);

            for (int i = 0; i < block.Body.Count; i++) {
                SyntaxNode node = block.Body[i];
                if (node is BlockStatement sub) {
                    Optimize(sub);
                } else if (node is IfStatement ifs) {
                    if (ifs.Consequent is BlockStatement cb) {
                        Optimize(cb);
                    }
                    if (ifs.Alternate is BlockStatement ab) {
                        Optimize(ab);
                    }
                } else if (node is AssignmentExpression assign) {
                    if (assign.Left is Identifier && assign.Operator == "=") {
                        RefactorConstantAssignments(assign, assign);
                        if (!assign.Replaced) {
                            RemoveUnusedAssignment(assign);
                        }
                        RefactorVariableName(assign, assign);
                    }
                }
            }
        }
    }
}
