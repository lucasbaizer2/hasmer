using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HbcUtil.Assembler.Parser {
    public interface IHasmTokenParser {
        bool CanParse(HasmStringStream stream);

        HasmToken Parse(HasmStringStream stream);
    }
}
