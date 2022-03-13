using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Analysis {
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

    /// <summary>
    /// Represents the control flow of a function as a graph.
    /// </summary>
    public class ControlFlowGraph {
        /// <summary>
        /// The root control flow block which contains a tree of all blocks that are contained within
        /// the function that this control flow graph was built for.
        /// </summary>
        public ControlFlowBlock RootBlock { get; set; }

        /// <summary>
        /// The Hermes bytecode file which the function is declared in.
        /// </summary>
        private HbcFile File;

        /// <summary>
        /// Represents a mapping between the <see cref="ControlFlowBlock.BaseOffset"/> of each block and the block object itself.
        /// </summary>
        private Dictionary<uint, ControlFlowBlock> CachedBlocks = new Dictionary<uint, ControlFlowBlock>();

        /// <summary>
        /// All of the instructions in the function.
        /// </summary>
        private List<HbcInstruction> Instructions;

        /// <summary>
        /// Creates a control flow graph by analyzing the given the instructions from a Hermes bytecode function.
        /// </summary>
        public ControlFlowGraph(HbcFile file, List<HbcInstruction> instructions) {
            File = file;
            Instructions = instructions;

            RootBlock = new ControlFlowBlock();
            CachedBlocks[RootBlock.BaseOffset] = RootBlock;

            BuildControlFlowBlocks();
            BuildBlockLengths();
        }

        /// <summary>
        /// Gets all of the instructions deifned within the given control flow block.
        /// </summary>
        public IEnumerable<HbcInstruction> GetBlockInstructions(ControlFlowBlock block) {
            return Instructions.Where(insn => insn.Offset >= block.BaseOffset && insn.Offset < block.BaseOffset + block.Length);
        }

        /// <summary>
        /// Returns the control flow block which contains the bytecode at the given offset. 
        /// </summary>
        private ControlFlowBlock GetBlockContainingOffset(uint offset) {
            if (CachedBlocks.Count == 1) {
                return CachedBlocks[0]; // return the starting block (offset = 0) if it's the only one
            }

            List<uint> keys = new List<uint>(CachedBlocks.Keys);
            keys.Sort();

            for (int i = 0; i < keys.Count - 1; i++) {
                uint before = CachedBlocks[keys[i]].BaseOffset;
                uint after = CachedBlocks[keys[i + 1]].BaseOffset;
                if (offset >= before && offset < after) {
                    return CachedBlocks[keys[i]];
                }
            }

            return CachedBlocks[keys[keys.Count - 1]];
        }

        private void BuildControlFlowBlocks() {
            HbcInstruction firstInstruction = Instructions[0];
            for (int i = 0; i < Instructions.Count; i++) {
                HbcInstruction insn = Instructions[i];
                HbcInstructionDefinition def = File.BytecodeFormat.Definitions[insn.Opcode];
                ControlFlowBlock currentBlock = GetBlockContainingOffset(insn.Offset);

                if (def.IsJump) {
                    int jump = insn.Operands[0].GetValue<int>();
                    uint toOffset = (uint)((int)insn.Offset + jump);
                    int toIndex = Instructions.FindIndex(insn => insn.Offset == toOffset);

                    if (toIndex == -1) {
                        throw new Exception("jump instruction points to illegal offset");
                    }

                    if (!CachedBlocks.ContainsKey(toOffset)) {
                        CachedBlocks[toOffset] = new ControlFlowBlock {
                            BaseOffset = toOffset
                        };
                    }
                    currentBlock.Consequent = toOffset;

                    if (def.OperandTypes.Count > 1) { // if there are more operands than just a jump address,
                                                      // that means it is a conditional jump and we need to add an alternate block
                        HbcInstruction nextInstruction = Instructions[i + 1];
                        uint nextOffset = nextInstruction.Offset;

                        if (!CachedBlocks.ContainsKey(nextOffset)) {
                            CachedBlocks[nextOffset] = new ControlFlowBlock {
                                BaseOffset = nextOffset
                            };
                        }
                        currentBlock.Alternate = nextOffset;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the <see cref="ControlFlowBlock.Length"/> property of all blocks in the function.
        /// The length is the amount of bytecode contained within each block,
        /// i.e. the amount of bytes from the offset until the next block.
        /// </summary>
        private void BuildBlockLengths() {
            List<uint> keys = new List<uint>(CachedBlocks.Keys);
            keys.Sort();

            for (int i = 0; i < keys.Count - 1; i++) {
                ControlFlowBlock block = CachedBlocks[keys[i]];
                ControlFlowBlock nextBlock = CachedBlocks[keys[i + 1]];
                block.Length = nextBlock.BaseOffset - block.BaseOffset;
            }

            uint totalLength = (uint) Instructions.Select(insn => (int)insn.Length).Sum();

            ControlFlowBlock lastBlock = CachedBlocks[keys[keys.Count - 1]];
            if (keys.Count == 1) {
                lastBlock.Length = totalLength;
            } else {
                ControlFlowBlock penultimateBlock = CachedBlocks[keys[keys.Count - 2]];
                lastBlock.Length = totalLength - penultimateBlock.BaseOffset;
            }
        }

        /// <summary>
        /// Converts the control flow graph into a readable string format showing each block, its instructions, and where it jumps to.
        /// </summary>
        public override string ToString() {
            SourceCodeBuilder str = new SourceCodeBuilder("  ");

            List<uint> keys = new List<uint>(CachedBlocks.Keys);
            keys.Sort();

            foreach (uint key in keys) {
                ControlFlowBlock block = CachedBlocks[key];
                str.Write($"### BLOCK OFFSET: {block.BaseOffset}");
                str.AddIndent(1);
                str.NewLine();

                foreach (HbcInstruction insn in GetBlockInstructions(block)) {
                    str.Write(insn.ToDisassembly(File));
                    str.NewLine();
                }

                str.AddIndent(-1);
                str.RemoveLastIndent();

                str.Write($"### CONSEQUENT: {block.Consequent?.ToString() ?? "None"}");
                str.NewLine();
                str.Write($"### ALTERNATE: {block.Alternate?.ToString() ?? "None"}");
                str.NewLine();

                str.NewLine();
            }

            return str.ToString();
        }
    }
}
