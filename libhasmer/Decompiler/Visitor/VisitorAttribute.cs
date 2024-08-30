using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.Visitor {
    /// <summary>
    /// Marks a class as containing methods that act as instruction vistors,
    /// i.e. marked with the <see cref="VisitorAttribute"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class VisitorCollectionAttribute : Attribute {
    }

    /// <summary>
    /// Marks a method as an instruction handler.
    /// The name of the method is the name of the instruction is handles.
    /// The instruction should have the signature of a <see cref="FunctionDecompiler.InstructionHandler"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class VisitorAttribute : Attribute {
    }
}
