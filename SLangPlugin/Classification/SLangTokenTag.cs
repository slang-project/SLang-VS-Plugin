using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin.Classification
{
    public class SLangTokenTag : ITag
    {
        public SLangTokenType type { get; private set; }

        public SLangTokenTag(SLangTokenType type)
        {
            this.type = type;
        }

    }

    [Export(typeof(ITaggerProvider))]
    [ContentType("SLang")]
    [TagType(typeof(SLangTokenTag))]
    internal sealed class SLangTokenTagProvider : ITaggerProvider
    {

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new SLangTokenTagger(buffer) as ITagger<T>;
        }
    }

    internal sealed class SLangTokenTagger : ITagger<SLangTokenTag>
    {
        ITextBuffer _buffer;

        internal SLangTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<SLangTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var currSpan in spans)
            {
                var snapshot = currSpan.Snapshot;

                SLang.Reader reader = new SLang.Reader(snapshot.GetText());
                SLang.Tokenizer tokenizer = new SLang.Tokenizer(reader, (SLang.Options)null);

                SLang.Token token = tokenizer.getNextToken();

                while (token.code != SLang.TokenCode.EOS)
                {
                    SLangTokenType tokenType = ClassificationMapping.getTokenType(token.code);
                    if (tokenType != SLangTokenType.Ignore)
                    {
                        Span currentTokenSpan = ConvertToSpan(token.span, snapshot);
                        SnapshotSpan tokenSpan = new SnapshotSpan(snapshot, currentTokenSpan);
                        if (tokenSpan.IntersectsWith(currSpan))
                            yield return new TagSpan<SLangTokenTag>(tokenSpan, new SLangTokenTag(tokenType));
                    }
                    token = tokenizer.getNextToken();
                }
            }
        }

        private Span ConvertToSpan(SLang.Span span, ITextSnapshot containingSnapshot)
        {
            int beginLine = span.begin.line - 1,
                endLine = span.end.line - 1,
                beginPos = span.begin.pos - 1,
                endPos = span.end.pos - 1;

            int begin = containingSnapshot.GetLineFromLineNumber(beginLine).Start + beginPos;
            int containingSpanshotEnd = containingSnapshot.GetLineFromLineNumber(containingSnapshot.LineCount - 1).End;
            int end = Math.Min(containingSnapshot.GetLineFromLineNumber(endLine).Start + endPos, containingSpanshotEnd);
            int length = end - begin + 1;
            return new Span(begin - 1, length);
        }
    }
}
