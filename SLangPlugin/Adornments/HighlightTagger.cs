using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SLangPlugin.Adornments
{
    #region Tagger Provider
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(Constants.ContentType)]
    [TagType(typeof(TextMarkerTag))]
    class HighlightTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            Func<ITagger<T>> creator = delegate () { return new HighlightTagger(textView, buffer) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(creator);
        }
    }
    #endregion

    #region Tagger
    /// <summary>
    /// Tagger that creates adornments for matching elements in code such as recurring identifiers and paren pairs.
    /// </summary>
    class HighlightTagger : ITagger<TextMarkerTag>
    {
        ITextView _textView;
        ITextBuffer _textBuffer;

        IList<ITagSpan<SLangTokenTag>> _lastTags;
        IList<ITagSpan<TextMarkerTag>> _lastHighlights;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public HighlightTagger(ITextView textView, ITextBuffer textBuffer)
        {
            _textView = textView;
            _textBuffer = textBuffer;
            
            SLangTokenTagger generalTagger = new SLangTokenTaggerProvider().CreateTagger<SLangTokenTag>(_textBuffer) as SLangTokenTagger;
            _lastTags = generalTagger._lastTags;
            
            // initialize empty highlights list
            _lastHighlights = new List<ITagSpan<TextMarkerTag>>();

            // add events for tracking view changes
            _textView.Caret.PositionChanged += CaretPositionChanged;
            _textView.LayoutChanged += ViewLayoutChanged;

            // make initial update
            UpdateAtCaretPosition(textView.Caret.Position);
        }

        void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot)
            {
                UpdateAtCaretPosition(_textView.Caret.Position);
            }
        }

        void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateAtCaretPosition(e.NewPosition);
        }


        private ITagSpan<SLangTokenTag> findRightMatch(SLang.TokenCode leftType, SLang.TokenCode rightType, int leftLocation,
            IList<ITagSpan<SLangTokenTag>> tagsList, IList<SLang.TokenCode> breakingTags = null)
        {
            int level = 0;
            for (var j = leftLocation + 1; j < tagsList.Count; ++j)
            {
                var tag = tagsList[j];
                if (tag.Tag.token.code == leftType)
                {
                    ++level;
                }
                else if (tag.Tag.token.code == rightType)
                {
                    if (level == 0) return tag;
                    --level;
                }
                else if (breakingTags != null && breakingTags.Contains(tag.Tag.token.code))
                {
                    return null;
                }
            }
            return null;
        }

        IList<Tuple<SLang.TokenCode, SLang.TokenCode>> matchPairs =
            new List<Tuple<SLang.TokenCode, SLang.TokenCode>>()
            {
                new Tuple<SLang.TokenCode, SLang.TokenCode>(SLang.TokenCode.LParen, SLang.TokenCode.RParen),
                new Tuple<SLang.TokenCode, SLang.TokenCode>(SLang.TokenCode.LBracket, SLang.TokenCode.RBracket),
                new Tuple<SLang.TokenCode, SLang.TokenCode>(SLang.TokenCode.Is, SLang.TokenCode.End),
                new Tuple<SLang.TokenCode, SLang.TokenCode>(SLang.TokenCode.Loop, SLang.TokenCode.End)
            };

        private ITagSpan<SLangTokenTag> findLeftMatch(SLang.TokenCode rightType, SLang.TokenCode leftType, int rightLocation,
            IList<ITagSpan<SLangTokenTag>> tagsList, IList<SLang.TokenCode> breakingTags = null)
        {
            int level = 0;
            for (var j = rightLocation - 1; j >= 0; --j)
            {
                var tag = tagsList[j];
                if (tag.Tag.token.code == rightType)
                {
                    ++level;
                }
                else if (tag.Tag.token.code == leftType)
                {
                    if (level == 0) return tag;
                    --level;
                }
                else if (breakingTags != null && breakingTags.Contains(tag.Tag.token.code))
                {
                    return null;
                }
            }
            return null;
        }

        bool isIn (CaretPosition caretPos, ITagSpan<SLangTokenTag> tag)
        {
            return caretPos.BufferPosition >= tag.Span.Start && caretPos.BufferPosition <= tag.Span.End;
        }

        bool isInExcludeLeft (CaretPosition caretPos, ITagSpan<SLangTokenTag> tag)
        {
            return caretPos.BufferPosition > tag.Span.Start && caretPos.BufferPosition <= tag.Span.End;
        }

        bool isInExcludeRight (CaretPosition caretPos, ITagSpan<SLangTokenTag> tag)
        {
            return caretPos.BufferPosition >= tag.Span.Start && caretPos.BufferPosition < tag.Span.End;
        }

        void UpdateAtCaretPosition(CaretPosition caretPosition)
        {
            _lastHighlights.Clear();

            //check empty char and break

            bool exitLoop = false;

            
            for (int i=0; i < _lastTags.Count; ++i)
            {
                ITagSpan<SLangTokenTag> tag = _lastTags[i];

                if (tag.Tag.token.code == SLang.TokenCode.Identifier)
                {
                    if (isIn(caretPosition, tag))
                    {
                        foreach (var innerTag in _lastTags)
                        {
                            if (innerTag.Tag.token.image.Equals(tag.Tag.token.image))
                            {
                                _lastHighlights.Add(new TagSpan<TextMarkerTag>(innerTag.Span, new TextMarkerTag("blue")));
                            }
                        }
                    }
                }
                foreach (var matchPair in matchPairs)
                {
                    
                    if (matchPair.Item1 == tag.Tag.token.code)
                    {
                        if (isInExcludeRight(caretPosition, tag))
                        {
                            ITagSpan<SLangTokenTag> leftMatch = tag;
                            ITagSpan<SLangTokenTag> rightMatch = findRightMatch(matchPair.Item1, matchPair.Item2, i, _lastTags);
                            if (rightMatch != null)
                            {
                                _lastHighlights.Add(new TagSpan<TextMarkerTag>(leftMatch.Span, new TextMarkerTag("blue")));
                                _lastHighlights.Add(new TagSpan<TextMarkerTag>(rightMatch.Span, new TextMarkerTag("blue")));
                            }
                            exitLoop = true;
                            break;
                        }
                    }
                    else if (matchPair.Item2 == tag.Tag.token.code)
                    {
                        if (isInExcludeLeft(caretPosition, tag))
                        {
                            ITagSpan<SLangTokenTag> rightMatch = tag;
                            ITagSpan<SLangTokenTag> leftMatch = findLeftMatch(matchPair.Item2, matchPair.Item1, i, _lastTags);
                            if (leftMatch != null)
                            {
                                _lastHighlights.Add(new TagSpan<TextMarkerTag>(leftMatch.Span, new TextMarkerTag("blue")));
                                _lastHighlights.Add(new TagSpan<TextMarkerTag>(rightMatch.Span, new TextMarkerTag("blue")));
                            }
                            exitLoop = true;
                            break;
                        }
                    }
                }
                if (exitLoop) break;
            }
            ITextSnapshot _snapshot = _textBuffer.CurrentSnapshot;
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_textBuffer.CurrentSnapshot, 0,
                    _textBuffer.CurrentSnapshot.Length)));
        }


        public IEnumerable<ITagSpan<TextMarkerTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tag in _lastHighlights)
            {
                yield return tag;
            }
        }
    }
    #endregion
}
