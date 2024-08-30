using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Analysis {
    /// <summary>
    /// The base class for performing static analysis on a decmopiled AST.
    /// This allows for optimizing the AST to make it as readable as possible.
    /// </summary>
    public class StaticAnalyzer {
        /// <summary>
        /// Optimizes declarations of objects in a block of code. For examplee, given the definition:
        /// <br />
        /// <code>
        /// obj = {} <br />
        /// obj.foo = 'bar'; <br />
        /// obj.baz = 'bam'; <br />
        /// </code>
        /// this method will convert the definiton into:
        /// <code>
        /// obj = { <br />
        ///     foo = 'bar', <br />
        ///     baz = 'bam' <br />
        /// } <br />
        /// </code>
        /// </summary>
        private static void OptimizeObjectDeclarations(BlockStatement block) {
            ObjectExpression currentObject = null;
            Identifier currentObjectName = null;

            SyntaxNode node = block.Body[0];
            while ((node = node.Next) != null) {
                if (node is AssignmentExpression assn && assn.Operator == "=") {
                    if (assn.Right is ObjectExpression expr && assn.Left is Identifier objName) {
                        currentObject = expr;
                        currentObjectName = objName;
                    } else if (currentObject != null &&
                          assn.Left is MemberExpression memberExpr &&
                          memberExpr.Object is Identifier objIndent &&
                          !objIndent.IsRedundant &&
                          objIndent.Name == currentObjectName.Name) {
                        currentObject.Properties.Add(new ObjectExpressionProperty {
                            Key = memberExpr.Property,
                            Value = assn.Right
                        });

                        node.ReplaceWith(new EmptyExpression()); // replace the assignment with an empty expression
                                                                 // this preserves the length of the body
                    } else {
                        currentObject = null;
                    }
                } else if (node is not EmptyExpression) {
                    currentObject = null;
                }
            }
        }

        /// <summary>
        /// Optimizes the condition tests and ordering of an if-else chain.
        /// </summary>
        private static void OptimizeIfStatement(IfStatement ifStatement) {
            if (ifStatement.Alternate is BlockStatement) { // if the alternate is just a simple else (i.e. has no conditions)
                if (ifStatement.Test is UnaryExpression unaryTest) {
                    if (unaryTest.Operator == "!") {
                        // if the entire test is wrapped in an invert operator
                        // switch the consequent and the alternate and drop the invert operator

                        ifStatement.Test = unaryTest.Argument; // remove the operator from the unary test

                        SyntaxNode tmpAlternate = ifStatement.Alternate;
                        ifStatement.Alternate = ifStatement.Consequent;
                        ifStatement.Consequent = tmpAlternate; // switch the consquent and the alternate
                    }
                }
            }
        }

        /// <summary>
        /// Optimizes all the statements witin a block of code.
        /// </summary>
        private static void OptimizeBlock(FunctionDeclaration func, BlockStatement block) {
            foreach (SyntaxNode node in block.Body) {
                if (node is IfStatement ifStatement) {
                    OptimizeIfStatement(ifStatement);
                    if (ifStatement.Consequent is BlockStatement cb) {
                        OptimizeBlock(func, cb);
                    }
                    if (ifStatement.Alternate is BlockStatement ab) {
                        OptimizeBlock(func, ab);
                    }
                } else if (node is BlockStatement blockStatement) {
                    OptimizeBlock(func, blockStatement);
                }
            }

            OptimizeObjectDeclarations(block);
        }

        /// <summary>
        /// Optimizes the declaration and body of a fuction.
        /// </summary>
        public static void OptimizeFunction(FunctionDeclaration func, DecompilerOptions options) {
            InstructionAnalyzer insnAnalyzer = new InstructionAnalyzer {
                Body = func.Body.Body,
                State = new StaticAnalyzerState(),
                Options = options
            };
            insnAnalyzer.Optimize(func.Body);

            if (func.HbcHeader.FunctionId == 0) {
                // add the declaration of the gloal variable to the start of the global function
                func.Body.Body.Insert(0, new AssignmentExpression {
                    Operator = "=",
                    DeclarationKind = "const",
                    Left = new Identifier("global"),
                    Right = new ObjectExpression()
                });

                // remove the "return" instruction from the global function
                for (int i = 0; i < func.Body.Body.Count; i++) {
                    SyntaxNode node = func.Body.Body[i];

                    if (node is ReturnStatement ret) {
                        // replace the return statement with just the value it's attempting to return
                        SyntaxNode arg = ret.Argument;
                        if (arg is not Literal && arg is not Identifier) {
                            func.Body.Body[i] = arg;
                        } else {
                            // remove return instructions which do not modify the state of the program
                            func.Body.Body[i] = new EmptyExpression();
                        }

                        break;
                    }
                }
            }

            OptimizeBlock(func, func.Body);
        }
    }
}
