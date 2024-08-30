using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hasmer.Assembler {
    /// <summary>
    /// Disassembles a function from Hermes bytecode into Hasm assembly.
    /// </summary>
    public class FunctionDisassembler {
        /// <summary>
        /// The disassembler being used.
        /// </summary>
        public HbcDisassembler Disassembler { get; set; }

        /// <summary>
        /// The file which declares the function being disassembled.
        /// </summary>
        public HbcFile Source => Disassembler.Source;

        /// <summary>
        /// The header of the function being disassembled.
        /// </summary>
        public HbcFuncHeader Func { get; set; }

        /// <summary>
        /// The parsed instructions of the function.
        /// </summary>
        private List<HbcInstruction> Instructions;

        /// <summary>
        /// The mapping of all labels to write. Index = offset of the label in bytes (like instruction offset), Value = name of the label at that offset.
        /// </summary>
        private Dictionary<uint, string> LabelTable = new Dictionary<uint, string>();

        /// <summary>
        /// The maximum amount of padding to add after each instruction before the operands are written.
        /// </summary>
        private int OpcodePadding;

        /// <summary>
        /// Creates a new FunctionDisassembler given the header of the function to disassemble.
        /// </summary>
        public FunctionDisassembler(HbcDisassembler disassembler, HbcFuncHeader func) {
            Disassembler = disassembler;
            Func = func;
            Instructions = func.Disassemble();
        }

        /// <summary>
        /// Regarding all instructions in the function, finds the mininum padding before the operands, placed after the operators.
        /// </summary>
        private void FindOpcodePadding() {
            foreach (HbcInstruction insn in Instructions) {
                string name = Source.BytecodeFormat.Definitions[insn.Opcode].Name;
                if (name.Length > OpcodePadding) {
                    OpcodePadding = name.Length;
                }
            }
            OpcodePadding++;
        }

        /// <summary>
        /// Gets the instruction that is jumped to by a jumping instruction and its register operand.
        /// <example>
        /// As an example, suppose GetJumpInstructionTarget was called regarding the "Jmp L2" instruction:
        /// <code>
        ///     LoadParam r3, 2 <br />                        
        ///     GetEnvironment r0, 1 <br />                        
        ///     LoadFromEnvironment r1, r0, 3 <br />                    
        ///     GetByIdShort r1, r1, 1, "default" <br />         
        ///     LoadConstNull r2 <br />
        ///     JNotEqual L1, r1, r2 <br />
        ///     LoadConstUndefined r1 <br />
        ///     Call2 r1, r3, r1, r2 <br />
        ///     Jmp L2 // &lt;--- this instruction <br /> <br />
        ///      
        ///     .label L1 <br />
        ///     LoadFromEnvironment r0, r0, 3 <br />                    
        ///     GetByIdShort r2, r0, 1, "default" <br />          
        ///     GetById r1, r2, 2, "getCurrentGrayscaleState" <br />
        ///     LoadParam r0, 1 <br />                        
        ///     Call3 r0, r1, r2, r0, r3 <br /><br />
        ///     
        ///     .label L2 <br /> 
        ///     LoadConstUndefined r0 // &lt;--- jumps here <br />
        ///     Ret r0
        /// </code>
        /// The returned offset will be that of "LoadConstUndefined".
        /// </example>
        /// </summary>
        private uint GetJumpInstructionTarget(HbcInstruction insn, HbcInstructionOperand operand) {
            string insnName = Source.BytecodeFormat.Definitions[insn.Opcode].Name;
            uint offset = operand.Type switch {
                HbcInstructionOperandType.Addr8 or HbcInstructionOperandType.Addr32 =>
                    (uint)((int)insn.Offset + operand.GetValue<int>()),
                _ => throw new InvalidOperationException("invalid jump operand")
            };
            return offset;
        }

        /// <summary>
        /// Finds all locations jumped to throughought the function's bytecode and builds a table of all instructions jumped to.
        /// </summary>
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

        /// <summary>
        /// Gets the type of a function per its flags.
        /// </summary>
        private string GetFunctionType() {
            if (Func.Flags.HasFlag(HbcFuncHeaderFlags.ProhibitNone)) {
                return "Function";
            } else if (Func.Flags.HasFlag(HbcFuncHeaderFlags.ProhibitConstruct)) {
                return "NCFunction";
            } else { // func is ProhibitCall
                return "Constructor";
            }
        }

        /// <summary>
        /// Annotates an instruction that refers to a closure.
        /// </summary>
        private string AnnotateClosure(uint closureIndex) {
            return $"Function <{Source.SmallFuncHeaders[closureIndex].GetFunctionName(Source)}>()";
        }

        /// <summary>
        /// Annotates an instruction that refers to the array buffer.
        /// </summary>
        private string AnnotateArray(uint arrayBufferIndex) {
            return $"Array Buffer Index = {arrayBufferIndex}, .data {DisassembleArray(arrayBufferIndex)}";
        }

        /// <summary>
        /// Returns the label-name of an item in the array buffer.
        /// </summary>
        private string DisassembleArray(uint arrayBufferIndex) {
            HbcDataBufferItems items = Disassembler.DataDisassembler.ArrayBuffer.Reverse<HbcDataBufferItems>().First(x => x.Offset <= arrayBufferIndex);
            int disasmIndex = Disassembler.DataDisassembler.ArrayBuffer.IndexOf(items);
            uint addedOffset = arrayBufferIndex - items.Offset;
            if (addedOffset != 0) {
                return $"A{disasmIndex}+{addedOffset}";
            }
            return $"A{disasmIndex}";
        }

        private string AnnotateObject(uint keyBufferIndex, uint valueBufferIndex, ushort length) {
            PrimitiveValue[] keys = Disassembler.DataDisassembler.GetElementSeries(Disassembler.DataDisassembler.KeyBuffer, keyBufferIndex, length);
            PrimitiveValue[] values = Disassembler.DataDisassembler.GetElementSeries(Disassembler.DataDisassembler.ValueBuffer, valueBufferIndex, length);

            JObject obj = new JObject();
            for (int i = 0; i < length; i++) {
                obj[keys[i].ToString()] = new JValue(values[i].RawValue);
            }

            return obj.ToString(Formatting.None);
        }

        /// <summary>
        /// Adds a comment to an instruction if neccessary.
        /// </summary>
        private void AnnotateVerbose(SourceCodeBuilder builder, HbcInstruction insn) {
            builder.Write("// offset = 0x");
            builder.Write(insn.Offset.ToString("X"));
            builder.Write(", length = ");
            builder.Write(insn.Length.ToString());

            HbcInstructionDefinition def = Source.BytecodeFormat.Definitions[insn.Opcode];
            if (def.IsJump) {
                builder.Write(", jump offset = 0x");
                builder.Write(insn.Operands[0].GetValue<int>().ToString("X"));
            }
        }

        /// <summary>
        /// Adds a comment to an instruction if neccessary.
        /// </summary>
        private void AnnotateInstruction(SourceCodeBuilder builder, HbcInstruction insn) {
            string annotation = Source.BytecodeFormat.Definitions[insn.Opcode].Name switch {
                "CreateClosure" => AnnotateClosure(insn.Operands[2].GetValue<ushort>()),
                "CreateClosureLongIndex" => AnnotateClosure(insn.Operands[2].GetValue<uint>()),
                "NewArrayWithBuffer" => AnnotateArray(insn.Operands[3].GetValue<ushort>()),
                "NewArrayWithBufferLong" => AnnotateArray(insn.Operands[3].GetValue<uint>()),
                // "NewObjectWithBuffer" => AnnotateObject(insn.Operands[3].GetValue<ushort>(), insn.Operands[4].GetValue<ushort>(), insn.Operands[2].GetValue<ushort>()),
                // "NewObjectWithBufferLong" => AnnotateObject(insn.Operands[3].GetValue<uint>(), insn.Operands[4].GetValue<uint>(), insn.Operands[2].GetValue<ushort>()),
                _ => null
            };

            if (annotation != null) {
                builder.Write("// ");
                builder.Write(annotation);
            }
        }

        /// <summary>
        /// Converts an operand to disassembly in the most human-readable format possible.
        /// </summary>
        private string OperandToDisassembly(HbcInstruction insn, int operandIndex) {
            HbcInstructionOperand operand = insn.Operands[operandIndex];
            return Source.BytecodeFormat.Definitions[insn.Opcode].Name switch {
                "NewArrayWithBuffer" when operandIndex == 3 => DisassembleArray(operand.GetValue<ushort>()),
                "NewArrayWithBufferLong" when operandIndex == 3 => DisassembleArray(operand.GetValue<uint>()),
                _ => operand.ToDisassembly(Source)
            };
        }

        /// <summary>
        /// Disassembles the function's header and bytecode into human and machine-readable disassembly.
        /// </summary>
        public string Disassemble() {
            FindOpcodePadding();
            BuildLabelTable();

            SourceCodeBuilder builder = new SourceCodeBuilder("    ");
            builder.Write(".start ");
            builder.Write(GetFunctionType());
            builder.Write(" <");
            builder.Write(Source.SmallFuncHeaders[Func.FunctionId].GetFunctionName(Source));
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

            List<uint> usedLabels = new List<uint>();
            foreach (HbcInstruction insn in Instructions) {
                if (LabelTable.ContainsKey(insn.Offset)) {
                    builder.NewLine();
                    builder.Write(".label ");
                    builder.Write(LabelTable[insn.Offset]);
                    builder.NewLine();

                    usedLabels.Add(insn.Offset);
                }

                int startLength = builder.Builder.Length;

                HbcInstructionDefinition def = Source.BytecodeFormat.Definitions[insn.Opcode];
                string name = def.Name;
                if (def.AbstractDefinition.HasValue && !Disassembler.Options.IsExact) {
                    name = Source.BytecodeFormat.AbstractDefinitions[def.AbstractDefinition.Value].Name;
                }
                builder.Write(name);

                int padding = OpcodePadding - name.Length;
                builder.Write(new string(' ', padding));

                for (int i = 0; i < insn.Operands.Count; i++) {
                    if (!Disassembler.Options.IsExact) {
                        // omit indentifier cache operands if not in exact mode
                        if (name == "TryGetById" || name == "GetById" || name == "TryPutById" || name == "PutById") {
                            if (i == 2) { // the third operand (i.e. insn.Operands[2]) is the identifier cache
                                continue; // just skip the operand; don't write it to the disassembly
                            }
                        }
                    }

                    HbcInstructionOperand operand = insn.Operands[i];
                    string readable = operand.Type switch {
                        HbcInstructionOperandType.Addr8 or HbcInstructionOperandType.Addr32 => LabelTable[GetJumpInstructionTarget(insn, operand)],
                        _ => OperandToDisassembly(insn, i)
                    };
                    builder.Write(readable);

                    if (i < insn.Operands.Count - 1) {
                        builder.Write(", ");
                    }
                }

                const int ANNOTATION_OFFSET = 50;
                int operationLength = builder.Builder.Length - startLength;
                int annotationPadding = Math.Max(1, ANNOTATION_OFFSET - operationLength);
                builder.Write(new string(' ', annotationPadding));

                if (Disassembler.Options.IsVerbose) {
                    AnnotateVerbose(builder, insn);

                    builder.NewLine();
                    builder.Write(new string(' ', operationLength + annotationPadding));
                }

                AnnotateInstruction(builder, insn);
                builder.NewLine();
            }

            builder.RemoveLastIndent();
            builder.AddIndent(-1);
            builder.Write(".end");

            foreach (uint offset in usedLabels) {
                LabelTable.Remove(offset);
            }
            if (LabelTable.Count > 0) {
                foreach (string labelName in LabelTable.Values) {
                    Console.WriteLine($"Error: could not find label destination in <{Source.SmallFuncHeaders[Func.FunctionId].GetFunctionName(Source)}>: {labelName}");
                }
            }

            return builder.ToString();
        }
    }
}
