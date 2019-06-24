using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin
{
    class ASTUtilities
    {
        static object m_SynLock = new object();
        public static List<SLang.DECLARATION> GetUnitsAndStandalones(ITextBuffer textBuffer)
        {
            lock (m_SynLock) {
                SLang.Reader reader = new SLang.Reader(textBuffer.CurrentSnapshot.GetText());
                SLang.Tokenizer tokenizer = new SLang.Tokenizer(reader, (SLang.Options)null);
                SLang.ENTITY.init(tokenizer);
                SLang.COMPILATION compilation = SLang.COMPILATION.parse();
                return compilation.units_and_standalones;
            }
        }
    }
}
