using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler {
    public class FunctionDisassembler {
        public HbcDisassembler Disassembler { get; set; }
        public HbcFile Source => Disassembler.Source;
        public HbcFuncHeader Func { get; set; }

        private List<HbcInstruction> Instructions;
        private Dictionary<uint, string> LabelTable = new Dictionary<uint, string>();
        private int OpcodePadding;

        public FunctionDisassembler(HbcDisassembler disassembler, HbcFuncHeader func) {
            Disassembler = disassembler;
            Func = func;
            Instructions = func.Disassemble().ToList();
        }

        private void FindOpcodePadding() {
            foreach (HbcInstruction insn in Instructions) {
                string name = Source.BytecodeFormat.Definitions[insn.Opcode].Name;
                if (name.Length > OpcodePadding) {
                    OpcodePadding = name.Length;
                }
            }
            OpcodePadding++;
        }

        private uint GetJumpInstructionTarget(HbcInstruction insn, HbcInstructionOperand operand) {
            return operand.Type switch {
                HbcInstructionOperandType.Addr8 => (uint)((int)insn.Offset + insn.Length + insn.Operands[0].GetValue<sbyte>()),
                HbcInstructionOperandType.Addr32 => (uint)((int)insn.Offset + insn.Length + insn.Operands[0].GetValue<int>()),
                _ => throw new InvalidOperationException("invalid jump operand")
            };
        }

        private void BuildLabelTable() {
            List<uint> labelOffsets = new List<uint>();
            foreach (HbcInstruction insn in Instructions) {
                IEnumerable<HbcInstructionOperand> jumpOperands = insn.Operands.Where(operand => operand.Type == HbcInstructionOperandType.Addr8 || operand.Type == HbcInstructionOperandType.Addr32);
                foreach (HbcInstructionOperand jumpOperand in jumpOperands) {
                    uint labelOffset = GetJumpInstructionTarget(insn, jumpOperand);
                    if (!labelOffsets.Contains(labelOffset)) {
                        labelOffsets.Add(labelOffset);
                    }
                }
            }
            labelOffsets.Sort();
            for (int i = 0; i < labelOffsets.Count; i++) {
                LabelTable[labelOffsets[i]] = $"L{i + 1}";
            }
        }

        private string GetFunctionType() {
            if (Func.Flags.HasFlag(HbcFuncHeaderFlags.ProhibitNone)) {
                return "Function";
            } else if (Func.Flags.HasFlag(HbcFuncHeaderFlags.ProhibitConstruct)) {
                return "NCFunction";
            } else { // func is ProhibitCall
                return "Constructor";
            }
        }

        private string GetFunctionName(uint id) {
            HbcSmallFuncHeader func = Source.SmallFuncHeaders[id];
            string functionName = Source.StringTable[func.FunctionName];
            if (functionName == "") {
                return $"$closure${Func.FunctionId}";
            }
            return functionName;
        }

        private string AnnotateClosure(uint closureIndex) {
            return $"Function <{GetFunctionName(closureIndex)}>()";
        }

        private string AnnotateArray(uint arrayBufferIndex, ushort elements) {
            PrimitiveValue[] read = Source.ArrayBuffer.Read(Source, arrayBufferIndex, elements, out HbcDataBufferTagType tagType);
            string stringified = $".data D{arrayBufferIndex} {tagType}[{string.Join<PrimitiveValue>(", ", read)}]";
            Disassembler.DataDeclarations[arrayBufferIndex] = stringified;

            return $".data D{arrayBufferIndex}";
        }

        private string AnnotateObject(uint keyBufferIndex, uint valueBufferIndex, ushort elements) {
            // PrimitiveValue[] keys = Source.ObjectKeyBuffer.Read(Source, keyBufferIndex, elements);
            // PrimitiveValue[] values = Source.ObjectValueBuffer.Read(Source, valueBufferIndex, elements);

            return "";
        }

        private void AnnotateInstruction(SourceCodeBuilder builder, HbcInstruction insn) {
            string annotation = Source.BytecodeFormat.Definitions[insn.Opcode].Name switch {
                "CreateClosure" => AnnotateClosure(insn.Operands[2].GetValue<ushort>()),
                "CreateClosureLongIndex" => AnnotateClosure(insn.Operands[2].GetValue<uint>()),
                "NewArrayWithBuffer" => AnnotateArray(insn.Operands[3].GetValue<ushort>(), insn.Operands[2].GetValue<ushort>()),
                "NewArrayWithBufferLong" => AnnotateArray(insn.Operands[3].GetValue<uint>(), insn.Operands[2].GetValue<ushort>()),
                // "NewObjectWithBuffer" => AnnotateObject(insn.Operands[3].GetValue<ushort>(), insn.Operands[4].GetValue<ushort>(), insn.Operands[2].GetValue<ushort>()),
                _ => null
            };

            if (annotation != null) {
                builder.Write("    # ");
                builder.Write(annotation);
            }
        }

        public string Disassemble() {
            FindOpcodePadding();
            BuildLabelTable();

            SourceCodeBuilder builder = new SourceCodeBuilder("    ");
            builder.Write(".start ");
            builder.Write(GetFunctionType());
            builder.Write(" <");
            builder.Write(GetFunctionName(Func.FunctionId));
            builder.Write(">(");
            for (int i = 0; i < Func.ParamCount; i++) {
                if (i == 0) {
                    builder.Write("this");
                } else {
                    builder.Write("par");
                    builder.Write(i.ToString());
                }
                if (i < Func.ParamCount - 1) {
                    builder.Write(", ");
                }
            }
            builder.Write(")");
            builder.AddIndent(1);
            builder.NewLine();

            builder.Write(".id ");
            builder.Write(Func.FunctionId.ToString());
            builder.NewLine();
            builder.Write(".params ");
            builder.Write(Func.ParamCount.ToString());
            builder.NewLine();
            builder.Write(".registers ");
            builder.Write(Func.FrameSize.ToString());
            builder.NewLine();
            builder.Write(".symbols ");
            builder.Write(Func.EnvironmentSize.ToString());
            builder.NewLine();
            if (Func.Flags.HasFlag(HbcFuncHeaderFlags.StrictMode)) {
                builder.Write(".strict");
                builder.NewLine();
            }
            builder.NewLine();

            foreach (HbcInstruction insn in Instructions) {
                if (LabelTable.ContainsKey(insn.Offset)) {
                    builder.NewLine();
                    builder.Write(".label ");
                    builder.Write(LabelTable[insn.Offset]);
                    builder.NewLine();
                }

                string name = Source.BytecodeFormat.Definitions[insn.Opcode].Name;
                builder.Write(name);

                int padding = OpcodePadding - name.Length;
                builder.Write(new string(' ', padding));

                for (int i = 0; i < insn.Operands.Count; i++) {
                    HbcInstructionOperand operand = insn.Operands[i];
                    string readable = operand.Type switch {
                        HbcInstructionOperandType.Addr8 or HbcInstructionOperandType.Addr32 => LabelTable[GetJumpInstructionTarget(insn, operand)],
                        _ => operand.ToDisassembly(Source)
                    };
                    builder.Write(readable);

                    if (i < insn.Operands.Count - 1) {
                        builder.Write(", ");
                    }
                }

                AnnotateInstruction(builder, insn);

                builder.NewLine();
            }

            builder.AddIndent(-1);
            builder.Builder.Remove(builder.Builder.Length - 4, 4);
            builder.Write(".end ");
            builder.Write(GetFunctionType());

            return builder.ToString();
        }
    }
}
