using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SLangPlugin.Outliner
{
    #region Tagger Provider

    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(Constants.ContentType)]
    internal sealed class OutliningTaggerProvider : ITaggerProvider
    {
        // Entities improted by MEF
        [Import]
        ITextEditorFactoryService _textEditorFactoryService = null;

        [Import]
        IEditorOptionsFactoryService _editorOptionsFactoryService = null;

        [Import]
        IProjectionBufferFactoryService _projectionBufferFactoryService = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        //[Import]
        IProjectionEditResolver projectionEditResolver = null;

        [Import]
        IContentTypeRegistryService contentTypeRegistryService = null;

        // Create a single tagger for each buffer.
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<SLangTokenTag> SLangTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<SLangTokenTag>(buffer);

            Func<ITagger<T>> creator = delegate () { return new OutliningTagger(buffer, _textEditorFactoryService,
                _editorOptionsFactoryService, _projectionBufferFactoryService, SLangTagAggregator, contentTypeRegistryService) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(creator);
        }
    }

    #endregion

    #region Tagger
    internal sealed class OutliningTagger : ITagger<IOutliningRegionTag>
    {

        string ellipsis = "...";    //the characters that are displayed when the region is collapsed
        string hoverText = "Unwrap block"; //the contents of the tooltip for the collapsed span
        ITextBuffer _buffer;
        ITextSnapshot _snapshot;
        ITextEditorFactoryService _textEditorFactoryService;
        IEditorOptionsFactoryService _editorOptionsFactoryService;
        IProjectionBufferFactoryService _projectionBufferFactoryService;
        IContentTypeRegistryService _contentTypeRegistryService;

        SLangTokenTagger _globalTagger;
        List<Region> _regions;
        IList<ITagSpan<SLangTokenTag>> _outlineTokens;
        ITagAggregator<SLangTokenTag> _aggregator;

        public OutliningTagger(ITextBuffer buffer, ITextEditorFactoryService textEditorFactoryService,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            IProjectionBufferFactoryService projectionBufferFactoryService,
            ITagAggregator<SLangTokenTag> SLangTagAggregator,
            IContentTypeRegistryService contentTypeRegistryService)
        {
            _buffer = buffer;
            _snapshot = buffer.CurrentSnapshot;
            _textEditorFactoryService = textEditorFactoryService;
            _editorOptionsFactoryService = editorOptionsFactoryService;
            _projectionBufferFactoryService = projectionBufferFactoryService;
            _contentTypeRegistryService = contentTypeRegistryService;
            _aggregator = SLangTagAggregator;

            var generalTagger = buffer.Properties.GetOrCreateSingletonProperty<ITagger<SLangTokenTag>>(
                creator: () => new SLangTokenTagger(buffer) as ITagger<SLangTokenTag>);

            SLangTokenTagger SLangTokenTagger = generalTagger as SLangTokenTagger;
            _outlineTokens = SLangTokenTagger._lastTags;
            _regions = new List<Region>();
            CreateOutlineRegions();
            _buffer.Changed += BufferChanged;
        }

        private IWpfTextView CreateElisionBufferView(ITextBuffer finalBuffer)
        {
            return CreateShrunkenTextView(_textEditorFactoryService, finalBuffer);
        }

        internal static IWpfTextView CreateShrunkenTextView(
            ITextEditorFactoryService textEditorFactoryService,
            ITextBuffer finalBuffer)
        {
            //var roles = textEditorFactoryService.CreateTextViewRoleSet(OutliningRegionTextViewRole);
            var view = textEditorFactoryService.CreateTextView(finalBuffer);

            view.Background = Brushes.Transparent;

            
            //view.SizeToFit();

            // Zoom out a bit to shrink the text.
            view.ZoomLevel *= 0.75;

            return view;
        }

        //private ITextBuffer CreateElisionBuffer()
        //{
        //    // Remove any starting whitespace.
        //    var span = TrimStartingNewlines(_hintSpan.GetSpan(_subjectBuffer.CurrentSnapshot));

        //    // Trim the length if it's too long.
        //    var shortSpan = span;
        //    if (span.Length > MaxPreviewText)
        //    {
        //        shortSpan = ComputeShortSpan(span);
        //    }

        //    // Create an elision buffer for that span, also trimming the
        //    // leading whitespace.
        //    var elisionBuffer = CreateElisionBufferWithoutIndentation(_subjectBuffer, shortSpan);
        //    var finalBuffer = elisionBuffer;

        //    // If we trimmed the length, then make a projection buffer that
        //    // has the above elision buffer and follows it with "..."
        //    if (span.Length != shortSpan.Length)
        //    {
        //        finalBuffer = CreateTrimmedProjectionBuffer(elisionBuffer);
        //    }

        //    return finalBuffer;
        //}

        //private ITextBuffer CreateTrimmedProjectionBuffer(ITextBuffer elisionBuffer)
        //{
        //    // The elision buffer is too long.  We've already trimmed it, but now we want to add
        //    // a "..." to it.  We do that by creating a projection of both the elision buffer and
        //    // a new text buffer wrapping the ellipsis.
        //    var elisionSpan = elisionBuffer.CurrentSnapshot.GetFullSpan();

        //    var sourceSpans = new List<object>()
        //            {
        //                elisionSpan.Snapshot.CreateTrackingSpan(elisionSpan, SpanTrackingMode.EdgeExclusive),
        //                Ellipsis
        //            };

        //    var projectionBuffer = _projectionBufferFactoryService.CreateProjectionBuffer(
        //        projectionEditResolver: null,
        //        sourceSpans: sourceSpans,
        //        options: ProjectionBufferOptions.None);

        //    return projectionBuffer;
        //}

        //private ITextBuffer CreateElisionBuffer(ITrackingSpan hintSpan, ITextBuffer subjectBuffer)
        //{
        //    // Remove any starting whitespace.
        //    var span = TrimStartingNewlines(hintSpan.GetSpan(subjectBuffer.CurrentSnapshot), subjectBuffer);

        //    // Trim the length if it's too long.
        //    var shortSpan = span;
        //    if (span.Length > 1000/*MaxPreviewText*/)
        //    {
        //        shortSpan = ComputeShortSpan(span, subjectBuffer);
        //    }

        //    // Create an elision buffer for that span, also trimming the
        //    // leading whitespace.
        //    var elisionBuffer = CreateElisionBufferWithoutIndentation(subjectBuffer, shortSpan);
        //    var finalBuffer = elisionBuffer;

        //    // If we trimmed the length, then make a projection buffer that
        //    // has the above elision buffer and follows it with "..."
        //    if (span.Length != shortSpan.Length)
        //    {
        //        finalBuffer = CreateTrimmedProjectionBuffer(elisionBuffer);
        //    }

        //    return finalBuffer;
        //}

        //public static SnapshotSpan GetFullSpan(this ITextSnapshot snapshot)
        //{
        //    //Contract.ThrowIfNull(snapshot);

        //    return new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
        //}
        //private ITextBuffer CreateTrimmedProjectionBuffer(ITextBuffer elisionBuffer)
        //{
        //    // The elision buffer is too long.  We've already trimmed it, but now we want to add
        //    // a "..." to it.  We do that by creating a projection of both the elision buffer and
        //    // a new text buffer wrapping the ellipsis.
        //    var elisionSpan = GetFullSpan(elisionBuffer.CurrentSnapshot);

        //    var sourceSpans = new List<object>()
        //            {
        //                elisionSpan.Snapshot.CreateTrackingSpan(elisionSpan, SpanTrackingMode.EdgeExclusive),
        //                ellipsis
        //            };

        //    var projectionBuffer = _projectionBufferFactoryService.CreateProjectionBuffer(
        //        projectionEditResolver: null,
        //        sourceSpans: sourceSpans,
        //        options: ProjectionBufferOptions.None);

        //    return projectionBuffer;
        //}

        //private Span ComputeShortSpan(Span span, ITextBuffer subjectBuffer)
        //{
        //    var endIndex = span.Start + 1000/*MaxPreviewText*/;
        //    var line = subjectBuffer.CurrentSnapshot.GetLineFromPosition(endIndex);

        //    return Span.FromBounds(span.Start, line.EndIncludingLineBreak);
        //}

        //private Span TrimStartingNewlines(Span span, ITextBuffer subjectBuffer)
        //{
        //    while (span.Length > 1 && char.IsWhiteSpace(subjectBuffer.CurrentSnapshot[span.Start]))
        //    {
        //        span = new Span(span.Start + 1, span.Length - 1);
        //    }

        //    return span;
        //}


        //FIXME: wrongly passed into classifier
        private ITextBuffer CreateElisionBuffer(ITextBuffer dataBuffer, Span shortHintSpan)
        {
            return _projectionBufferFactoryService.CreateElisionBuffer(
                projectionEditResolver: null,
                contentType: _contentTypeRegistryService.GetContentType(Constants.ContentType),
                exposedSpans: new NormalizedSnapshotSpanCollection(dataBuffer.CurrentSnapshot, shortHintSpan),
                options: ElisionBufferOptions.None);
        }


        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;
            List<Region> currentRegions = _regions;
            ITextSnapshot currentSnapshot = _snapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);
            int startLineNumber = entire.Start.GetContainingLine().LineNumber;
            int endLineNumber = entire.End.GetContainingLine().LineNumber;
            foreach (var region in currentRegions ?? Enumerable.Empty<Region>())
            {
                if (region.StartLine <= endLineNumber &&
                    region.EndLine >= startLineNumber)
                {
                    var startLine = currentSnapshot.GetLineFromLineNumber(region.StartLine);
                    var endLine = currentSnapshot.GetLineFromLineNumber(region.EndLine);

                    SnapshotSpan regionSpan = new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);


                    var elisionBuffer = CreateElisionBuffer(_buffer, regionSpan);
                    var vi = CreateElisionBufferView(elisionBuffer);

                    var hover = regionSpan.GetText();
                    yield return new TagSpan<IOutliningRegionTag>(
                        regionSpan,
                        new OutliningRegionTag(false, false, ellipsis, vi));
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        void BufferChanged(object sender, TextContentChangedEventArgs args)
        {
            // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).
            if (args.After != _buffer.CurrentSnapshot)
                return;

            _snapshot = _buffer.CurrentSnapshot;
            CreateOutlineRegions();
        }

        // Step #9: Add a method that parses the buffer. The example given here is for illustration only. 
        // It synchronously parses the buffer into nested outlining regions.


        int MinIndexOfSeveral(string space, IList<string> tags, StringComparison comparator)
        {
            int min = -1;
            foreach (var tag in tags)
            {
                var curr = space.IndexOf(tag, comparator);
                if (curr >= 0 && curr < min)
                    min = curr; // select minimum non-negative index
                else if (min < 0 && curr >= 0)
                    min = curr;

            }
            return min;
        }

        // TODO: consider deletion
        //void ReParse()
        //{
        //    ITextSnapshot newSnapshot = _buffer.CurrentSnapshot;
        //    List<Region> newRegions = new List<Region>();

        //    //keep the current (deepest) partial region, which will have
        //    // references to any parent partial regions.
        //    PartialRegion currentRegion = null;

        //    foreach (var line in newSnapshot.Lines)
        //    {
        //        int regionStart = -1;
        //        string text = line.GetText();

        //        //lines that contain "do/..." denote the start of a new region.
        //        if ((regionStart = MinIndexOfSeveral(text, startHideList, StringComparison.Ordinal)) != -1)
        //        {
        //            int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
        //            int newLevel;
        //            if (!TryGetLevel(text, regionStart, out newLevel))
        //                newLevel = currentLevel + 1;

        //            //levels are the same and we have an existing region;
        //            //end the current region and start the next
        //            if (currentLevel == newLevel && currentRegion != null)
        //            {
        //                newRegions.Add(new Region()
        //                {
        //                    Level = currentRegion.Level,
        //                    StartLine = currentRegion.StartLine,
        //                    StartOffset = currentRegion.StartOffset,
        //                    EndLine = line.LineNumber
        //                });

        //                currentRegion = new PartialRegion()
        //                {
        //                    Level = newLevel,
        //                    StartLine = line.LineNumber,
        //                    StartOffset = regionStart,
        //                    PartialParent = currentRegion.PartialParent
        //                };
        //            }
        //            //this is a new (sub)region
        //            else
        //            {
        //                currentRegion = new PartialRegion()
        //                {
        //                    Level = newLevel,
        //                    StartLine = line.LineNumber,
        //                    StartOffset = regionStart,
        //                    PartialParent = currentRegion
        //                };
        //            }
        //        }
        //        //lines that contain "end" denote the end of a region
        //        else if ((regionStart = text.IndexOf(endHide, StringComparison.Ordinal)) != -1)
        //        {
        //            int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
        //            int closingLevel;
        //            if (!TryGetLevel(text, regionStart, out closingLevel))
        //                closingLevel = currentLevel;

        //            //the regions match
        //            if (currentRegion != null &&
        //                currentLevel == closingLevel)
        //            {
        //                newRegions.Add(new Region()
        //                {
        //                    Level = currentLevel,
        //                    StartLine = currentRegion.StartLine,
        //                    StartOffset = currentRegion.StartOffset,
        //                    EndLine = line.LineNumber
        //                });

        //                currentRegion = currentRegion.PartialParent;
        //            }
        //        }
        //    }

        //    //determine the changed span, and send a changed event with the new spans
        //    List<Span> oldSpans =
        //        new List<Span>(_regions.Select(r => AsSnapshotSpan(r, _snapshot)
        //            .TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive)
        //            .Span));
        //    List<Span> newSpans =
        //            new List<Span>(newRegions.Select(r => AsSnapshotSpan(r, newSnapshot).Span));

        //    NormalizedSpanCollection oldSpanCollection = new NormalizedSpanCollection(oldSpans);
        //    NormalizedSpanCollection newSpanCollection = new NormalizedSpanCollection(newSpans);

        //    //the changed regions are regions that appear in one set or the other, but not both.
        //    NormalizedSpanCollection removed =
        //    NormalizedSpanCollection.Difference(oldSpanCollection, newSpanCollection);

        //    int changeStart = int.MaxValue;
        //    int changeEnd = -1;

        //    if (removed.Count > 0)
        //    {
        //        changeStart = removed[0].Start;
        //        changeEnd = removed[removed.Count - 1].End;
        //    }

        //    if (newSpans.Count > 0)
        //    {
        //        changeStart = Math.Min(changeStart, newSpans[0].Start);
        //        changeEnd = Math.Max(changeEnd, newSpans[newSpans.Count - 1].End);
        //    }

        //    _snapshot = newSnapshot;
        //    _regions = newRegions;

        //    if (changeStart <= changeEnd)
        //    {
        //        ITextSnapshot snap = _snapshot;
        //        if (this.TagsChanged != null)
        //            this.TagsChanged(this, new SnapshotSpanEventArgs(
        //                new SnapshotSpan(_snapshot, Span.FromBounds(changeStart, changeEnd))));
        //    }
        //}

        void CreateOutlineRegions()
        {
            ITextSnapshot newSnapshot = _buffer.CurrentSnapshot;
            List<Region> newRegions = new List<Region>();

            //keep the current (deepest) partial region, which will have
            // references to any parent partial regions.
            PartialRegion currentRegion = null;

            foreach (var outlineTokenSpan in _outlineTokens ?? Enumerable.Empty<ITagSpan<SLangTokenTag>>())
            {
                var outlineToken = outlineTokenSpan.Tag.token;
                // Handle opening token
                if (Constants.outlineStartTokenTypes.Contains(outlineToken.code))
                {
                    int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
                    int newLevel = currentLevel + 1;

                    currentRegion = new PartialRegion()
                    {
                        Level = newLevel,
                        StartLine = outlineToken.span.end.line - 1,
                        StartOffset = outlineToken.span.end.pos - 2,
                        PartialParent = currentRegion
                    };
                }
                // Handle closing token
                else if (Constants.outlineEndTokenTypes.Contains(outlineToken.code))
                {
                    int currentLevel = (currentRegion != null) ? currentRegion.Level : 1;
                    int closingLevel = currentLevel;

                    if (currentRegion != null)
                    {
                        newRegions.Add(new Region()
                        {
                            Level = currentLevel,
                            StartLine = currentRegion.StartLine,
                            StartOffset = currentRegion.StartOffset,
                            EndLine = outlineToken.span.end.line - 1
                        });

                        currentRegion = currentRegion.PartialParent;
                    }
                }
            }

            _regions = newRegions;
        }

        // TODO: consider deletion
        //// Step #10: The following helper method gets an integer that represents the level of the outlining, 
        //// such that 1 is the leftmost brace pair.
        //static bool TryGetLevel(string text, int startIndex, out int level)
        //{
        //    level = -1;
        //    if (text.Length > startIndex + 3)
        //    {
        //        if (int.TryParse(text.Substring(startIndex + 1), out level))
        //            return true;
        //    }

        //    return false;
        //}

        //static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
        //{
        //    var startLine = snapshot.GetLineFromLineNumber(region.StartLine);
        //    var endLine = (region.StartLine == region.EndLine) ? startLine
        //         : snapshot.GetLineFromLineNumber(region.EndLine);
        //    return new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);
        //}

        // Step #12: The following code is for illustration only. 
        // It defines a PartialRegion class that contains 
        // the line number and offset of the start of an outlining region, and also a reference to the parent 
        // region (if any). This enables the parser to set up nested outlining regions. A derived Region class 
        // contains a reference to the line number of the end of an outlining region.
        class PartialRegion
        {
            public int StartLine { get; set; }
            public int StartOffset { get; set; }
            public int Level { get; set; } // TODO: consider deletion
            public PartialRegion PartialParent { get; set; }
        }

        class Region : PartialRegion
        {
            public int EndLine { get; set; }
        }
    }
    #endregion
}
