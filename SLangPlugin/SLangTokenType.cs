using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin
{
    public enum SLangTokenType
    {
        Identifier, Keyword, Comment, Operator, StringLiteral, NumericLiteral, Other, Ignore, Whitespace,
        Unit
    }
}
