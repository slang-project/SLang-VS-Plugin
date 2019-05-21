using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SLang;

namespace SLangPlugin.Classification
{
    public static class ClassificationMapping
    {
        private static readonly Dictionary<SLang.TokenCode, SLangTokenType> TokenMapping
            = new Dictionary<SLang.TokenCode, SLangTokenType>()
        {
            { TokenCode.Identifier, SLangTokenType.Identifier },
            { TokenCode.LComment, SLangTokenType.Comment },
            { TokenCode.DComment, SLangTokenType.Comment },
            { TokenCode.SComment, SLangTokenType.Comment },
            { TokenCode.String, SLangTokenType.StringLiteral },
            { TokenCode.If, SLangTokenType.Keyword}
        };

        public static SLangTokenType getTokenType(SLang.TokenCode code)
        {
            if (TokenMapping.ContainsKey(code))
                return TokenMapping[code];
            else return SLangTokenType.Ignore;
        }
    }
}
