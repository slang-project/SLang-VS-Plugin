using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin
{
    class Constants
    {
        public const string ContentType = "SLang",
                            SingleCommentChars = "//",
                            MultilineCommentStartChars = "/*",
                            MultilineCommentEndChars = "*/";

        //// if user insers/deletes one of following chars
        //// it's possible that colorization for several chars may be changed
        //public static readonly List<char> CharsCausingMultilineRetag =
        //    new List<char>() { '*', '/' };

        // TODO: rewrite to more intelligent structure
        // should be exclusive:
        public static readonly HashSet<SLang.TokenCode> outlineStartTokenTypes =
            new HashSet<SLang.TokenCode>() { SLang.TokenCode.Loop, SLang.TokenCode.Then, SLang.TokenCode.Is };
        public static readonly HashSet<SLang.TokenCode> outlineEndTokenTypes =
            new HashSet<SLang.TokenCode>() { SLang.TokenCode.End }; 
    }
}
