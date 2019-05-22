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

            { TokenCode.EOS, SLangTokenType.Ignore },              //  end of source
            { TokenCode.EOL, SLangTokenType.Ignore },
            { TokenCode.Tab, SLangTokenType.Ignore },              //  horizontal tabulation
            { TokenCode.Blank, SLangTokenType.Whitespace },        //  blank (whitespace)
            { TokenCode.SComment, SLangTokenType.Comment },        //  short comment
            { TokenCode.LComment, SLangTokenType.Comment },        //  long comment
            { TokenCode.DComment, SLangTokenType.Comment },        //  documenting comment
            { TokenCode.ERROR, SLangTokenType.Ignore },            //  illegal token
            { TokenCode.Identifier, SLangTokenType.Identifier },   //
            { TokenCode.Integer, SLangTokenType.NumericLiteral },  //
            { TokenCode.Real, SLangTokenType.NumericLiteral },     //
            { TokenCode.String,  SLangTokenType.StringLiteral },   //
            { TokenCode.Character, SLangTokenType.StringLiteral }, //
            { TokenCode.Comma, SLangTokenType.Ignore },            //  ,
            { TokenCode.Dot, SLangTokenType.Ignore },              //  .
            { TokenCode.DotDot, SLangTokenType.Ignore },           //  ..
            { TokenCode.Semicolon, SLangTokenType.Ignore },        //  ;
            { TokenCode.Colon, SLangTokenType.Ignore },            //  :
            { TokenCode.LParen, SLangTokenType.Ignore },           //  (
            { TokenCode.RParen, SLangTokenType.Ignore },           //  )
            { TokenCode.LBracket, SLangTokenType.Ignore },         //  [
            { TokenCode.RBracket, SLangTokenType.Ignore },         //  ]
    //      { TokenCode.LBrace, SLangTokenType.Ignore },       //  {
    //      { TokenCode.RBrace, SLangTokenType.Ignore },       //  }
            { TokenCode.Arrow, SLangTokenType.Ignore },         //  =>
            { TokenCode.Arrow2, SLangTokenType.Ignore },       //  ->
            { TokenCode.Assign, SLangTokenType.Ignore },       //  :=
            { TokenCode.Equal, SLangTokenType.Ignore },        //  =
            { TokenCode.EqualEqual, SLangTokenType.Ignore },   //  ==
            { TokenCode.NotEqual, SLangTokenType.Ignore },     //  /=
            { TokenCode.NotEqualDeep, SLangTokenType.Ignore }, //  /==
            { TokenCode.Less, SLangTokenType.Ignore },         //  <
            { TokenCode.LessEqual, SLangTokenType.Ignore },    //  <=
            { TokenCode.Greater, SLangTokenType.Ignore },      //  >
            { TokenCode.GreaterEqual, SLangTokenType.Ignore }, //  >=
            { TokenCode.Tilde, SLangTokenType.Ignore },        //  ~
            { TokenCode.Question, SLangTokenType.Ignore },     //  ?
            { TokenCode.Vertical, SLangTokenType.Ignore },     //  |
            { TokenCode.VertVert, SLangTokenType.Ignore },     //  ||
            { TokenCode.Caret, SLangTokenType.Ignore },        //  ^
            { TokenCode.Ampersand, SLangTokenType.Ignore },    //  &
            { TokenCode.AmpAmp, SLangTokenType.Ignore },       //  &&
            { TokenCode.Plus, SLangTokenType.Ignore },         //  +
            { TokenCode.Minus, SLangTokenType.Ignore },        //  -
            { TokenCode.Multiply, SLangTokenType.Ignore },     //  *
            { TokenCode.Power, SLangTokenType.Ignore },        //  **
            { TokenCode.Divide, SLangTokenType.Ignore },       //  /
         // { TokenCode.Remainder, SLangTokenType.Ignore },    //  %
            { TokenCode.Remainder, SLangTokenType.Ignore },    //  \
         // { TokenCode.Call, SLangTokenType.Ignore },         //  ()
            { TokenCode.PlusEqual, SLangTokenType.Ignore },    //  +=
            { TokenCode.MinusEqual, SLangTokenType.Ignore },   //  -=
            { TokenCode.PlusPlus, SLangTokenType.Ignore },     //  ++
            { TokenCode.MinusMinus, SLangTokenType.Ignore },   //  --
            { TokenCode.MultEqual, SLangTokenType.Ignore },    //  *=
            { TokenCode.Abstract, SLangTokenType.Keyword },
            { TokenCode.Alias, SLangTokenType.Keyword },
       //   { TokenCode.And, SLangTokenType.Keyword },
            { TokenCode.As, SLangTokenType.Keyword },
            { TokenCode.Break, SLangTokenType.Keyword },
            { TokenCode.Case, SLangTokenType.Keyword },
            { TokenCode.Catch, SLangTokenType.Keyword },
            { TokenCode.Check, SLangTokenType.Keyword },
            { TokenCode.Concurrent, SLangTokenType.Keyword },
            { TokenCode.Const, SLangTokenType.Keyword },
            { TokenCode.Else, SLangTokenType.Keyword },
            { TokenCode.Elsif, SLangTokenType.Keyword },
            { TokenCode.End, SLangTokenType.Keyword },
            { TokenCode.Ensure, SLangTokenType.Keyword },
            { TokenCode.Extend, SLangTokenType.Keyword },
         // { TokenCode.External, SLangTokenType.Keyword },
            { TokenCode.Final, SLangTokenType.Keyword },
            { TokenCode.Foreign, SLangTokenType.Keyword },
            { TokenCode.Hidden, SLangTokenType.Keyword },
            { TokenCode.If, SLangTokenType.Keyword },
            { TokenCode.In, SLangTokenType.Keyword },
            { TokenCode.Init, SLangTokenType.Keyword },
            { TokenCode.Invariant, SLangTokenType.Keyword },
            { TokenCode.Is, SLangTokenType.Keyword },
            { TokenCode.Lambda, SLangTokenType.Keyword },
            { TokenCode.Loop, SLangTokenType.Keyword },
            { TokenCode.New, SLangTokenType.Keyword },
       //   { TokenCode.Not, SLangTokenType.Keyword },
            { TokenCode.Old, SLangTokenType.Keyword },
       //   { TokenCode.Or, SLangTokenType.Keyword },
            { TokenCode.Override, SLangTokenType.Keyword },
            { TokenCode.Package, SLangTokenType.Keyword },
            { TokenCode.Private, SLangTokenType.Keyword },
            { TokenCode.Public, SLangTokenType.Keyword },
            { TokenCode.Pure, SLangTokenType.Keyword },
            { TokenCode.Raise, SLangTokenType.Keyword },
            { TokenCode.Ref, SLangTokenType.Keyword },
            { TokenCode.Require, SLangTokenType.Keyword },
            { TokenCode.Return, SLangTokenType.Keyword },
            { TokenCode.Routine, SLangTokenType.Keyword },
            { TokenCode.Safe, SLangTokenType.Keyword },
            { TokenCode.Then, SLangTokenType.Keyword },
            { TokenCode.This, SLangTokenType.Keyword },
            { TokenCode.Try, SLangTokenType.Keyword },
            { TokenCode.Unit, SLangTokenType.Keyword },
            { TokenCode.Use, SLangTokenType.Keyword },
            { TokenCode.Val, SLangTokenType.Keyword },
            { TokenCode.Variant, SLangTokenType.Keyword },
            { TokenCode.While, SLangTokenType.Keyword },
       //   { TokenCode.Xor SLangTokenType.Keyword }
        };

        public static SLangTokenType getTokenType(SLang.TokenCode code)
        {
            if (TokenMapping.ContainsKey(code))
                return TokenMapping[code];
            else return SLangTokenType.Ignore;
        }
    }
}
