using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer {
    /// <summary>
    /// Utility for working with string escape codes.
    /// </summary>
    public class StringEscape {
        // Taken from https://stackoverflow.com/a/33697376.
        private const string DoubleFixedPoint = "0.###################################################################################################################################################################################################################################################################################################################################################";

        /// <summary>
        /// Escapes a string so that it can be used as string literal in Hasm source code.
        /// Shamelessly taken from https://stackoverflow.com/a/14087738.
        /// </summary>
        public static string Escape(string s) {
            StringBuilder literal = new StringBuilder(s.Length + 2);
            literal.Append("\"");
            foreach (char c in s) {
                switch (c) {
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        if (c >= 0x20 && c <= 0x7e) {
                            // ASCII printable character
                            literal.Append(c);
                        } else {
                            // UTF16 escaped character
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            literal.Append("\"");
            return literal.ToString();
        }

        public static string DoubleToString(double d) {
            if (double.IsInfinity(d)) {
                return "Infinity";
            } else if (double.IsNegativeInfinity(d)) {
                return "-Infinity";
            } else if (double.IsNaN(d)) {
                return "NaN";
            } else {
                return d.ToString(DoubleFixedPoint);
            }
        }
    }
}
