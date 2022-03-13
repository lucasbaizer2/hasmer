using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer.Assembler {
    /// <summary>
    /// Used for diassembling the data section of a Hermes bytecode file.
    /// </summary>
    public class DataDisassembler {
        /// <summary>
        /// The Hermes bytecode file being disassembled.
        /// </summary>
        public HbcFile Source { get; set; }

        /// <summary>
        /// The decoded Array Buffer.
        /// </summary>
        public List<HbcDataBufferItems> ArrayBuffer { get; set; }

        /// <summary>
        /// The decoded Object Key Buffer.
        /// </summary>
        public List<HbcDataBufferItems> KeyBuffer { get; set; }

        /// <summary>
        /// The decoded Object Value Buffer.
        /// </summary>
        public List<HbcDataBufferItems> ValueBuffer { get; set; }

        /// <summary>
        /// Creates a new DataDisassembler for a given Hermes bytecode file.
        /// </summary>
        public DataDisassembler(HbcFile source) {
            Source = source;
        }

        /// <summary>
        /// Returns an array containing *length* items starting at buffer offset *offset* in the given buffer.
        /// If *length* extends over multiple entries in the array buffer (i.e. multiple data declarations),
        /// the elements from all entries are returned in order.
        /// This enables reading over multiple entries at once.
        /// </summary>
        public PrimitiveValue[] GetElementSeries(List<HbcDataBufferItems> buffer, uint offset, uint length) {
            PrimitiveValue[] series = new PrimitiveValue[length];
            int currentIndex = 0;
            for (int i = 0; i < length; i++) {
                HbcDataBufferItems items = buffer.Find(item => item.Offset == offset);
                series[i] = items.Items[currentIndex++];

                if (currentIndex >= items.Items.Length) {
                    int nextIndex = buffer.IndexOf(items) + 1;
                    if (nextIndex == buffer.Count) {
                        // exit early as to not access outside the bounds of `buffer`
                        if (i == length - 1) {
                            return series;
                        }

                        // if we didn't read all the expected items until `length`
                        // throw an exception
                        throw new IndexOutOfRangeException("length");
                    }
                    offset = buffer[nextIndex].Offset;
                    currentIndex = 0;
                }
            }
            return series;
        }

        /// <summary>
        /// Writes an entire buffer (i.e. key/array/value/etc.) as disassembly.
        /// </summary>
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
            if (buffer.Count > 0) {
                builder.AppendLine();
            }
        }

        public void DisassembleData() {
            ArrayBuffer = Source.ArrayBuffer.ReadAll(Source);
            KeyBuffer = Source.ObjectKeyBuffer.ReadAll(Source);
            ValueBuffer = Source.ObjectValueBuffer.ReadAll(Source);
        }

        /// <summary>
        /// Disassembles the Hermes bytecode data buffers and returns a string representing the disassembly.
        /// </summary>
        public string Disassemble() {
            StringBuilder builder = new StringBuilder();

            DisassembleData();
            AppendDisassembly(builder, ArrayBuffer, 'A');
            AppendDisassembly(builder, KeyBuffer, 'K');
            AppendDisassembly(builder, ValueBuffer, 'V');

            return builder.ToString();
        }
    }
}
