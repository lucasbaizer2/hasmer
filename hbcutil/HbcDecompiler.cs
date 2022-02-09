using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HbcUtil {
    public class HbcDecompiler {
        public HbcFile Source { get; private set; }

        private CodeBuilder CodeBuilder = new CodeBuilder("    ");
        private Dictionary<uint, string> StringTable;

        public HbcDecompiler(HbcFile source) {
            Source = source;

            InitializeStorage();

            foreach (HbcSmallFuncHeader func in Source.SmallFuncHeaders) {
                DecompileFunction(func);
                break; // only do first function for now
            }
        }

        private string DecompileFunctionSignature(HbcFuncHeader func) {
            string name = StringTable[func.FunctionName];
            uint paramCount = func.ParamCount;

            StringBuilder builder = new StringBuilder();
            builder.Append("function ");
            builder.Append(name);
            builder.Append("(");

            for (uint i = 0; i < paramCount; i++) {
                builder.Append("par");
                builder.Append(i + 1);
                if (i < paramCount - 1) {
                    builder.Append(", ");
                }
            }

            builder.Append(")");

            return builder.ToString();
        }

        private List<HbcInstruction> DisassembleFunction(HbcFuncHeader func) {
            foreach (HbcInstruction insn in func.Disassemble()) {
                Console.WriteLine(insn.Opcode);
                Console.WriteLine(Source.BytecodeFormat.Definitions[insn.Opcode].Name);
                Console.WriteLine(string.Join(", ", insn.Operands));
            }
            return func.Disassemble().ToList();
        }

        private void DecompileFunction(HbcSmallFuncHeader func) {
            string signature = DecompileFunctionSignature(func);
            Console.WriteLine(signature);
            List<HbcInstruction> insns = DisassembleFunction(func);
            Console.WriteLine(JsonConvert.SerializeObject(insns));
        }

        private void InitializeStorage() {
            const uint MAX_STRING_LENGTH = (1 << 8) - 1;

            StringTable = new Dictionary<uint, string>(Source.SmallStringTable.Count);
            for (uint i = 0; i < Source.SmallStringTable.Count; i++) {
                HbcSmallStringTableEntry entry = Source.SmallStringTable[(int)i];

                uint offset = entry.Offset;
                uint length = entry.Length;
                uint isUTF16 = entry.IsUTF16;

                if (length >= MAX_STRING_LENGTH) {
                    HbcOverflowStringTableEntry overflow = Source.OverflowStringTable[(int)offset];
                    offset = overflow.Offset;
                    length = overflow.Length;
                }

                if (isUTF16 == 1) {
                    length *= 2;
                }

                byte[] stringBytes = new byte[length];
                Array.Copy(Source.StringStorage, offset, stringBytes, 0, length);

                string str = isUTF16 switch {
                    1 => string.Concat(stringBytes.Select(b => b.ToString("X2"))),
                    _ => Encoding.UTF8.GetString(stringBytes)
                };
                StringTable[i] = str;
            }
        }
    }
}
