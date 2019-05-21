using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace SLangPlugin.Classification
{
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

    public class SLangTokenTag : ITag
    {
        public SLangTokenType type { get; private set; }

        public SLangTokenTag(SLangTokenType type)
        {
            this.type = type;
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

            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();

                string wholeView = curSpan.GetText();

                int curLoc = containingLine.Start.Position;

                SLang.Reader reader = new SLang.Reader(containingLine.GetText());
                SLang.Tokenizer tokenizer = new SLang.Tokenizer(reader, (SLang.Options)null);

                SLang.Token token = tokenizer.getNextToken();

                while (token.code != SLang.TokenCode.EOS)
                {
                    SLangTokenType tokenType = ClassificationMapping.getTokenType(token.code);
                    if (tokenType != SLangTokenType.Ignore)
                    {
                        SLang.Span span = token.span;
                        int size = span.end.pos - span.begin.pos + 1;
                        SnapshotSpan tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc + span.begin.pos - 2, size));
                        if (tokenSpan.IntersectsWith(curSpan))
                            yield return new TagSpan<SLangTokenTag>(tokenSpan, new SLangTokenTag(tokenType));
                    }
                    token = tokenizer.getNextToken();
                }
            }
        }
    }
}
