using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hasmer.Decompiler.AST;

namespace Hasmer.Decompiler.Analysis {
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
            BuildDefaultConsequents();
        }

        /// <summary>
        /// Gets all of the instructions deifned within the given control flow block.
        /// </summary>
        public IEnumerable<HbcInstruction> GetBlockInstructions(ControlFlowBlock block) {
            return Instructions.Where(insn => insn.Offset >= block.BaseOffset && insn.Offset < block.BaseOffset + block.Length);
        }

        /// <summary>
        /// Gets the <see cref="ControlFlowBlockType"/> of a given block in the graph.
        /// </summary>
        public ControlFlowBlockType GetBlockType(ControlFlowBlock block) {
            List<ControlFlowBlock> consequentPointers = CachedBlocks.Values.Where(other => other.Consequent == block.BaseOffset).ToList();
            List<ControlFlowBlock> alternatePointers = CachedBlocks.Values.Where(other => other.Alternate == block.BaseOffset).ToList();

            if (consequentPointers.Count == 0 && alternatePointers.Count == 0) {
                // if the block is not pointed to by any other blocks, it is general code
                return ControlFlowBlockType.General;
            }

            if (consequentPointers.Count >= 2 || alternatePointers.Count >= 2) {
                // if the block is pointed to by multiple other blocks, it is a general block
                return ControlFlowBlockType.General;
            }

            if (consequentPointers.Count == 1 && alternatePointers.Count == 1) {
                // if the block is pointed to as both a consequent and an alternate, it is a general block
                return ControlFlowBlockType.General;
            }

            if (consequentPointers.Count == 1) {
                return ControlFlowBlockType.IfStatement;
            }

            if (alternatePointers.Count == 1) {
                return ControlFlowBlockType.ElseStatement;
            }

            throw new Exception("unreachable");
        }

        /// <summary>
        /// Returns the control flow block whose <see cref="ControlFlowBlock.BaseOffset"/> is exactly equal to the given offset,
        /// or `null` if there is no block which starts at the offset.
        /// </summary>
        public ControlFlowBlock GetBlockAtOffset(uint offset) {
            return CachedBlocks.GetValueOrDefault(offset, null);
        }

        /// <summary>
        /// Returns the control flow block which contains the bytecode at the given offset. 
        /// </summary>
        public ControlFlowBlock GetBlockContainingOffset(uint offset) {
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

            uint totalLength = (uint)Instructions.Select(insn => (int)insn.Length).Sum();

            ControlFlowBlock lastBlock = CachedBlocks[keys[keys.Count - 1]];
            if (keys.Count == 1) {
                lastBlock.Length = totalLength;
            } else {
                ControlFlowBlock penultimateBlock = CachedBlocks[keys[keys.Count - 2]];
                lastBlock.Length = totalLength - penultimateBlock.BaseOffset;
            }
        }

        /// <summary>
        /// Sets the <see cref="ControlFlowBlock.Consequent"/> property for all the blocks whose last instruction is not a jump or `Ret` instruction.
        /// The consequent offset is set to be the instruction immediately after the last instruction in the non-jumping/non-returning block.
        /// </summary>
        private void BuildDefaultConsequents() {
            List<uint> keys = new List<uint>(CachedBlocks.Keys);
            keys.Sort();

            for (int i = 0; i < keys.Count - 1; i++) {
                ControlFlowBlock block = CachedBlocks[keys[i]];
                ControlFlowBlock nextBlock = CachedBlocks[keys[i + 1]];

                HbcInstruction lastInsn = GetBlockInstructions(block).Last();
                HbcInstructionDefinition lastDef = File.BytecodeFormat.Definitions[lastInsn.Opcode];
                if (!lastDef.IsJump && lastDef.Name != "Ret") {
                    block.Consequent = nextBlock.BaseOffset;
                }
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
                str.Write($"### BLOCK OFFSET: {block.BaseOffset} --- BLOCK TYPE: {GetBlockType(block)}");
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
