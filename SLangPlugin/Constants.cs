using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin
{
    class Constants
    {
        public const string ContentType = "SLang";
        public const string SingleCommentChars = "//";

        // should be exclusive:
        public static readonly HashSet<SLang.TokenCode> outlineStartTokenTypes = new HashSet<SLang.TokenCode>() { SLang.TokenCode.Loop, SLang.TokenCode.Then };
        public static readonly HashSet<SLang.TokenCode> outlineEndTokenTypes = new HashSet<SLang.TokenCode>() { SLang.TokenCode.End }; 
    }
}
