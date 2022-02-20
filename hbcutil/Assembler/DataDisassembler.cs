using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler {
    public class DataDisassembler {
        public HbcDisassembler Disassembler { get; set; }
        public HbcFile Source => Disassembler.Source;

        public List<HbcDataBufferItems> ArrayBuffer { get; set; }
        public List<HbcDataBufferItems> KeyBuffer { get; set; }
        public List<HbcDataBufferItems> ValueBuffer { get; set; }

        public DataDisassembler(HbcDisassembler disassembler) {
            Disassembler = disassembler;
        }

        public PrimitiveValue[] GetElementSeries(List<HbcDataBufferItems> buffer, uint offset, uint length) {
            PrimitiveValue[] series = new PrimitiveValue[length];
            int currentIndex = 0;
            for (int i = 0; i < length; i++) {
                HbcDataBufferItems items = buffer.Find(item => item.Offset == offset);
                series[i] = items.Items[currentIndex++];

                if (currentIndex >= items.Items.Length) {
                    offset = buffer[buffer.IndexOf(items) + 1].Offset;
                    currentIndex = 0;
                }
            }
            return series;
        }

        private void AppendDisassembly(StringBuilder builder, List<HbcDataBufferItems> buffer, char prefix) {
            for (int i = 0; i < buffer.Count; i++) {
                HbcDataBufferItems items = buffer[i];
                IEnumerable<PrimitiveValue> mapped = items.Items.Select(x => {
                    if (x.TypeCode == TypeCode.String) {
                        x.SetValue('"' + x.GetValue<string>().Replace("\"", "\\\"") + '"');
                    }
                    return x;
                });
                string tagType = items.Prefix.TagType switch {
                    HbcDataBufferTagType.ByteString or HbcDataBufferTagType.ShortString or HbcDataBufferTagType.LongString => "String",
                    _ => items.Prefix.TagType.ToString()
                };
                string joined = string.Join(", ", mapped);
                builder.AppendLine($".data {prefix}{i} {tagType}[{joined}] # offset = {items.Offset}");
            }
            builder.AppendLine();
        }

        public string Disassemble() {
            StringBuilder builder = new StringBuilder();

            ArrayBuffer = Source.ArrayBuffer.ReadAll(Source);
            AppendDisassembly(builder, ArrayBuffer, 'A');
            KeyBuffer = Source.ObjectKeyBuffer.ReadAll(Source);
            AppendDisassembly(builder, KeyBuffer, 'K');
            ValueBuffer = Source.ObjectValueBuffer.ReadAll(Source);
            AppendDisassembly(builder, ValueBuffer, 'V');

            return builder.ToString();
        }
    }
}
