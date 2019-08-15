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
    #region Tag Types
    /// <summary>
    /// Language tokens distiguished by editor tools
    /// </summary>
    public enum SLangTokenType
    {
        Identifier, Keyword, Comment, Operator, StringLiteral,
        NumericLiteral, Other, Ignore, Whitespace, Unit
    }
    #endregion

    #region Tag Structure
    /// <summary>
    /// Container that represents single token
    /// </summary>
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

    public class SLangTokenTagSpanEqualityComparer : IEqualityComparer<ITagSpan<SLangTokenTag>>
    {
        public bool Equals(ITagSpan<SLangTokenTag> x, ITagSpan<SLangTokenTag> y)
        {
            // TODO: remove this mess, make a better equality comparison, probably inside token itself
            var codeComp = x.Tag.token.code.Equals(y.Tag.token.code);
            var spanComp = x.Tag.token.span.begin.line.Equals(y.Tag.token.span.begin.line) &&
                           x.Tag.token.span.begin.pos.Equals(y.Tag.token.span.begin.pos) &&
                           x.Tag.token.span.end.line.Equals(y.Tag.token.span.end.line) &&
                           x.Tag.token.span.end.pos.Equals(y.Tag.token.span.end.pos);
            return codeComp && spanComp;
        }

        public int GetHashCode(ITagSpan<SLangTokenTag> obj)
        {
            return obj.Tag.token.code.GetHashCode() ^
                   obj.Tag.token.span.GetHashCode();
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
            Func<ITagger<T>> creator = () => { return new SLangTokenTagger(buffer) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(creator);
        }
    }
    #endregion

    #region Tagger
    /// <summary>
    /// Main tags source for other tools, produces tags with full information.
    /// </summary>
    internal sealed class SLangTokenTagger : ITagger<SLangTokenTag>
    {
        ITextBuffer _buffer;
        ITextSnapshot _snapshot;
        
        public IList<ITagSpan<SLangTokenTag>> _currentTags = new List<ITagSpan<SLangTokenTag>>(),
                                              _previousTags = new List<ITagSpan<SLangTokenTag>>();

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

            // scan the document and extract all tags
            PerformReTag();

            // find what regions have acually changed
            var comp = new SLangTokenTagSpanEqualityComparer();
            var symmetricDifference = _previousTags.Except(_currentTags, comp)
                                .Union(_currentTags.Except(_previousTags, comp));

            // stop if nothing to update
            if (!symmetricDifference.Any()) return;

            // transform for updater
            var symDiffSpans = new NormalizedSnapshotSpanCollection(
                            symmetricDifference.Select(tag => tag.Span
                            .TranslateTo(_buffer.CurrentSnapshot, SpanTrackingMode.EdgeExclusive)));

            // inform about new tags information
            foreach(SnapshotSpan span in symDiffSpans)
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));

        }

        private void PerformReTag()
        {
            _previousTags = new List<ITagSpan<SLangTokenTag>>(_currentTags);
            _currentTags.Clear();
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
                    _currentTags.Add(new TagSpan<SLangTokenTag>(tokenSpan, currentTag));
                }
                token = tokenizer.getNextToken();
            }
        }

        public IEnumerable<ITagSpan<SLangTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count > 0)
            {
                foreach (var tagSpan in _currentTags ?? Enumerable.Empty<ITagSpan<SLangTokenTag>>())
                {
                    yield return tagSpan;
                }
            }
        }

        private Span ConvertToSpan(SLang.Span span, ITextSnapshot containingSnapshot)
        {
            // TODO: Justify magic numbers
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
