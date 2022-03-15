using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Decompiler.Analysis {
    /// <summary>
    /// The type of structure that a <see cref="ControlFlowBlock"/> represents.
    /// </summary>
    public enum ControlFlowBlockType {
        /// <summary>
        /// The control flow block is not within a control flow structure.
        /// </summary>
        General,

        /// <summary>
        /// The control flow block is within an if or if-else statement in an if/if-else/else chain.
        /// </summary>
        IfStatement,

        /// <summary>
        /// The control flow block is within an else statement in an if/if-else/else chian.
        /// </summary>
        ElseStatement
    }

    /// <summary>
    /// Represents a block of code in a <see cref="ControlFlowGraph"/>.
    /// The block of code begins at either a position that is jumped to, or the start of the function.
    /// The block of code ends at the first jump instruction found after the code beyond the start position, 
    /// or the end of the function.
    /// </summary>
    public class ControlFlowBlock {
        /// <summary>
        /// The offset of the first instruction in the control flow block.
        /// </summary>
        public uint BaseOffset { get; set; }

        /// <summary>
        /// The bytecode length of the instructions in the control flow block.
        /// </summary>
        public uint Length { get; set; }

        /// <summary>
        /// The bytecode offset of the block that is jumped to as a consequence of this block ending.
        /// The consequent block could be either a destination in an unconditional jump (e.g. the Jmp instruction),
        /// or a destination of a conditional jump (e.g. the JNotEqual instruction).
        /// <br />
        /// If this block ends with a returning instruction (e.g. the Ret instruction),
        /// then the Consequent will be null, representing that this block does not execute further code in the function.
        /// </summary>
        public uint? Consequent { get; set; }

        /// <summary>
        /// The bytecode offset of the block that is executed as a consequence of this block ending without jumping.
        /// The alternate block is the code that is executed immediately after a conditional jump instruction
        /// (e.g. the JNotEqual instruction) when the jumping operation is not executed (i.e. it does not jump to another block).
        /// </summary>
        public uint? Alternate { get; set; }
    }
}
