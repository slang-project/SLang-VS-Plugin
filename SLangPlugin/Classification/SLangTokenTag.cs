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

            if (spans.Count > 0)
            {
                var curSpan = spans[0];

            //foreach (SnapshotSpan curSpan in spans)
            //{
                //ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();

                ITextSnapshot wholeSpanSnapshot = curSpan.Snapshot;

                //int curLoc = containingLine.Start.Position;

                

                SLang.Reader reader = new SLang.Reader(wholeSpanSnapshot.GetText());
                SLang.Tokenizer tokenizer = new SLang.Tokenizer(reader, (SLang.Options)null);

                SLang.Token token = tokenizer.getNextToken();

                while (token.code != SLang.TokenCode.EOS)
                {
                    SLangTokenType tokenType = ClassificationMapping.getTokenType(token.code);
                    if (tokenType != SLangTokenType.Ignore)
                    {
                        Span currentTokenSpan = ConvertToSpan(token.span, wholeSpanSnapshot);
                        SnapshotSpan tokenSpan = new SnapshotSpan(wholeSpanSnapshot, currentTokenSpan);
                        if (tokenSpan.IntersectsWith(curSpan))
                            yield return new TagSpan<SLangTokenTag>(tokenSpan, new SLangTokenTag(tokenType));
                    }
                    token = tokenizer.getNextToken();
                }
            }
        }

        /*
         * TODO: write description
         */
        private Span ConvertToSpan(SLang.Span span, ITextSnapshot containingSnapshot)
        {
            int beginLine = span.begin.line,
                endLine = span.end.line,
                beginPos = span.begin.pos,
                endPos = span.end.pos;

            int begin = containingSnapshot.GetLineFromLineNumber(beginLine - 1).Start + beginPos;
            int end = System.Math.Min(containingSnapshot.GetLineFromLineNumber(endLine - 1).Start + endPos, containingSnapshot.GetLineFromLineNumber(containingSnapshot.LineCount - 1).End);
            int length = end - begin + 1;
            return new Span(begin - 2, length);
        }
    }
}
