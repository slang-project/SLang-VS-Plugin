using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin
{
    #region Tag Structure
    public class SLangTokenTag : ITag
    {
        public SLangTokenType type { get; private set; }
        public SLang.Token token { get; private set; } 

        public SLangTokenTag(SLang.Token token)
        {
            this.token = token;
            this.type = Classification.ClassificationMapping.getTokenType(token.code);
        }

    }
    #endregion

    #region Tagger Provider

    [Export(typeof(ITaggerProvider))]
    [ContentType(Constants.ContentType)]
    [TagType(typeof(SLangTokenTag))]
    internal sealed class SLangTokenTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            Func<ITagger<T>> creator = delegate () { return new SLangTokenTagger(buffer) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(creator);
        }
    }
    #endregion

    #region Tagger
    internal sealed class SLangTokenTagger : ITagger<SLangTokenTag>
    {
        ITextBuffer _buffer;
        ITextSnapshot _snapshot;
        public readonly IList<ITagSpan<SLangTokenTag>> _lastTags = new List<ITagSpan<SLangTokenTag>>();

        internal SLangTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _snapshot = buffer.CurrentSnapshot;
            PerformReTag();
            _buffer.Changed += BufferChanged;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private void BufferChanged(object caller, TextContentChangedEventArgs args)
        {
            // if current change does not correspond to latest version of buffer, ignore until newer
            if (args.After != _buffer.CurrentSnapshot)
                return;

            _snapshot = _buffer.CurrentSnapshot;

            PerformReTag();
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_snapshot.GetLineFromLineNumber(0).Start, 
                _snapshot.GetLineFromLineNumber(_snapshot.LineCount - 1).End).TranslateTo(_snapshot, SpanTrackingMode.EdgeExclusive)));
        }

        private void PerformReTag()
        {
            _lastTags.Clear();
            SLang.Reader reader = new SLang.Reader(_snapshot.GetText());
            SLang.Tokenizer tokenizer = new SLang.Tokenizer(reader, (SLang.Options)null);

            SLang.Token token = tokenizer.getNextToken();

            while (token.code != SLang.TokenCode.EOS)
            {
                SLangTokenTag currentTag = new SLangTokenTag(token);
                if (currentTag.type != SLangTokenType.Whitespace)
                {
                    Span currentTokenSpan = ConvertToSpan(token.span, _snapshot);
                    SnapshotSpan tokenSpan = new SnapshotSpan(_snapshot, currentTokenSpan);
                    _lastTags.Add(new TagSpan<SLangTokenTag>(tokenSpan, currentTag));
                }
                token = tokenizer.getNextToken();
            }
        }

        public IEnumerable<ITagSpan<SLangTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count > 0)
            {
                foreach (var tagSpan in _lastTags ?? Enumerable.Empty<ITagSpan<SLangTokenTag>>())
                {
                    yield return tagSpan;
                }
            }
        }

        private Span ConvertToSpan(SLang.Span span, ITextSnapshot containingSnapshot)
        {
            int beginLine = span.begin.line - 1,
                endLine = span.end.line - 1,
                beginPos = span.begin.pos - 2,
                endPos = span.end.pos - 2;

            int begin = containingSnapshot.GetLineFromLineNumber(beginLine).Start + beginPos;
            int containingSpanshotEnd = containingSnapshot.GetLineFromLineNumber(containingSnapshot.LineCount - 1).End;
            int proposedLastLine = Math.Min(endLine, containingSnapshot.LineCount - 1);
            int proposedLastLineStart = containingSnapshot.GetLineFromLineNumber(proposedLastLine).Start;
            int end = Math.Min(proposedLastLineStart + endPos, containingSpanshotEnd);
            int length = end - begin;
            return new Span(begin, length);
        }
    }
    #endregion
}
