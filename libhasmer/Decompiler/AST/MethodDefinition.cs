﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.AST {
    public class MethodDefinition : SyntaxNode {
        public SyntaxNode Key { get; set; }
        public FunctionDeclaration Value { get; set; }
        public bool IsStatic { get; set; }
        public bool IsComputed { get; set; }

        public override void WriteDirect(SourceCodeBuilder builder) {
            Value.Name = null;
            Value.IsExpression = true;

            if (IsStatic) {
                builder.Write("static ");
            }
            if (IsComputed) {
                builder.Write("[");
            }
            Key.Write(builder);
            if (IsComputed) {
                builder.Write("]");
            }
            Value.Write(builder);
        }
    }
}
