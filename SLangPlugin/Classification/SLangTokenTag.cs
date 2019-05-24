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
        ITextSnapshot _snapshot;
        IList<ITagSpan<SLangTokenTag>> _lastTags = new List<ITagSpan<SLangTokenTag>>();

        internal SLangTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _snapshot = buffer.CurrentSnapshot;
            PerformReTag();
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private void PerformReTag()
        {
            _lastTags.Clear();
            SLang.Reader reader = new SLang.Reader(_snapshot.GetText());
            SLang.Tokenizer tokenizer = new SLang.Tokenizer(reader, (SLang.Options)null);

            SLang.Token token = tokenizer.getNextToken();

            while (token.code != SLang.TokenCode.EOS)
            {
                SLangTokenType tokenType = ClassificationMapping.getTokenType(token.code);
                if (tokenType != SLangTokenType.Ignore)
                {
                    Span currentTokenSpan = ConvertToSpan(token.span, _snapshot);
                    SnapshotSpan tokenSpan = new SnapshotSpan(_snapshot, currentTokenSpan);
                    _lastTags.Add(new TagSpan<SLangTokenTag>(tokenSpan, new SLangTokenTag(tokenType)));
                }
                token = tokenizer.getNextToken();
            }

        }

        public IEnumerable<ITagSpan<SLangTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count > 0)
            {
                var snap = spans[0].Snapshot;
                if (snap != _snapshot)
                {
                    _snapshot = snap;
                    PerformReTag();
                    TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_snapshot.GetLineFromLineNumber(0).Start, _snapshot.GetLineFromLineNumber(_snapshot.LineCount - 1).End)));
                }

                foreach (var tagSpan in _lastTags)
                {
                    yield return tagSpan;
                }
            }
            //foreach (var currSpan in spans)
            //{
                
                
            //}
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
