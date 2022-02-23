using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil {
    /// <summary>
    /// Utility for working with string escape codes.
    /// </summary>
    public class StringEscape {
        /// <summary>
        /// Escapes a string, replacing untypable characters and keyword operators with escaped versions.
        /// </summary>
        public static string Escape(string str) {
            return str
                .Replace("\n", @"\n")
                .Replace("\r", @"\r")
                .Replace("\"", "\\\"");
        }

        /// <summary>
        /// Unescapes a string, replacing escape codes with their corresponding untypable characters or keyword operators.
        /// </summary>
        public static string Unescape(string str) {
            return str
                .Replace(@"\n", "\n")
                .Replace(@"\r", "\r")
                .Replace("\\\"", "\"");
        }
    }
}
