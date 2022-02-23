using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil {
    public class StringEscape {
        public static string Escape(string str) {
            return str
                .Replace("\n", @"\n")
                .Replace("\r", @"\r")
                .Replace("\"", "\\\"");
        }

        public static string Unescape(string str) {
            return str
                .Replace(@"\n", "\n")
                .Replace(@"\r", "\r")
                .Replace("\\\"", "\"");
        }
    }
}
