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
using System.Windows;
using System.Windows.Media;

// code for creation of tooltip preview is taken from https://github.com/dotnet/roslyn/blob/master/src/EditorFeatures/Core.Wpf/Structure/BlockTagState.cs

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

        [Import]
        IContentTypeRegistryService contentTypeRegistryService = null;


        // Create a single tagger for each buffer.
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<SLangTokenTag> SLangTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<SLangTokenTag>(buffer);

            Func<ITagger<T>> creator = () => { return new OutliningTagger(buffer, _textEditorFactoryService,
                _editorOptionsFactoryService, _projectionBufferFactoryService, SLangTagAggregator, contentTypeRegistryService) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(creator);
        }
    }

    #endregion

    #region Tagger
    internal sealed class OutliningTagger : ITagger<IOutliningRegionTag>
    {

        string ellipsis = "...";    //the characters that are displayed when the region is collapsed
        const int MaxPreviewSize = 1000;
        const double PreviewWindowZoomFactor = 1.0;

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

            SLangTokenTagger generalTagger = new SLangTokenTaggerProvider().CreateTagger<SLangTokenTag>(_buffer) as SLangTokenTagger;
            _outlineTokens = generalTagger._currentTags;


            _regions = new List<Region>();
            CreateOutlineRegions();
            _buffer.Changed += BufferChanged;
        }

        private IWpfTextView CreateElisionBufferView(ITextBuffer finalBuffer)
        {
            return CreateShrunkenTextView(_textEditorFactoryService, finalBuffer);
        }
        private const string OutliningRegionTextViewRole = nameof(OutliningRegionTextViewRole);

        public static IWpfTextView SizeToFit(IWpfTextView view)
        {
            void firstLayout(object sender, TextViewLayoutChangedEventArgs args)
            {
            var newHeight = view.LineHeight * view.TextBuffer.CurrentSnapshot.LineCount;
            if (IsGreater(newHeight, view.VisualElement.Height))
            {
                view.VisualElement.Height = newHeight;
            }

            var newWidth = view.MaxTextRightCoordinate;
            if (IsGreater(newWidth, view.VisualElement.Width))
            {
                view.VisualElement.Width = newWidth;
            }
                view.LayoutChanged -= firstLayout;
            }

            view.LayoutChanged += firstLayout;

            bool IsGreater(double value, double other)
                => IsNormal(value) && (!IsNormal(other) || value > other);

            bool IsNormal(double value)
                => !double.IsNaN(value) && !double.IsInfinity(value);

            return view;
        }

        internal static IWpfTextView CreateShrunkenTextView(
            ITextEditorFactoryService textEditorFactoryService,
            ITextBuffer finalBuffer)
        {
            ITextViewRoleSet roles = textEditorFactoryService.CreateTextViewRoleSet(OutliningRegionTextViewRole);
            IWpfTextView view = textEditorFactoryService.CreateTextView(finalBuffer, roles);

            view.Background = Brushes.Transparent;
            view = SizeToFit(view);
            view.ZoomLevel *= PreviewWindowZoomFactor;

            return view;
        }

        private ITextBuffer CreateElisionBuffer(ITrackingSpan hintSpan, ITextBuffer subjectBuffer)
        {
            // Remove any starting whitespace.
            var span = TrimStartingNewlines(hintSpan.GetSpan(subjectBuffer.CurrentSnapshot), subjectBuffer);

            // Trim the length if it's too long.
            var shortSpan = span;
            if (span.Length > MaxPreviewSize)
            {
                shortSpan = ComputeShortSpan(span, subjectBuffer);
            }

            // Create an elision buffer for that span, also trimming the
            // leading whitespace.
            var elisionBuffer = CreateElisionBufferWithoutIndentation(shortSpan, subjectBuffer);
            var finalBuffer = elisionBuffer;

            // If we trimmed the length, then make a projection buffer that
            // has the above elision buffer and follows it with "..."
            if (span.Length != shortSpan.Length)
            {
                finalBuffer = CreateTrimmedProjectionBuffer(elisionBuffer);
            }

            return finalBuffer;
        }

        public static SnapshotSpan GetFullSpan(ITextSnapshot snapshot)
        {
            return new SnapshotSpan(snapshot, new Span(0, snapshot.Length));
        }

        private ITextBuffer CreateTrimmedProjectionBuffer(ITextBuffer elisionBuffer)
        {
            // The elision buffer is too long.  We've already trimmed it, but now we want to add
            // a "..." to it.  We do that by creating a projection of both the elision buffer and
            // a new text buffer wrapping the ellipsis.
            var elisionSpan = GetFullSpan(elisionBuffer.CurrentSnapshot);

            var sourceSpans = new List<object>()
                    {
                        elisionSpan.Snapshot.CreateTrackingSpan(elisionSpan, SpanTrackingMode.EdgeExclusive),
                        ellipsis
                    };

            var projectionBuffer = _projectionBufferFactoryService.CreateProjectionBuffer(
                projectionEditResolver: null,
                sourceSpans: sourceSpans,
                options: ProjectionBufferOptions.None);

            return projectionBuffer;
        }

        private Span ComputeShortSpan(Span span, ITextBuffer subjectBuffer)
        {
            var endIndex = span.Start + MaxPreviewSize;
            var line = subjectBuffer.CurrentSnapshot.GetLineFromPosition(endIndex);

            return Span.FromBounds(span.Start, line.EndIncludingLineBreak);
        }

        private Span TrimStartingNewlines(Span span, ITextBuffer subjectBuffer)
        {
            while (span.Length > 1 && char.IsWhiteSpace(subjectBuffer.CurrentSnapshot[span.Start]))
            {
                span = new Span(span.Start + 1, span.Length - 1);
            }

            return span;
        }

        private ITextBuffer CreateElisionBufferWithoutIndentation(Span shortHintSpan, ITextBuffer dataBuffer)
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
                    SnapshotSpan viewSpan = new SnapshotSpan(startLine.Start, endLine.End);
                    ITrackingSpan trackingViewSpan = currentSnapshot.CreateTrackingSpan(viewSpan, SpanTrackingMode.EdgeInclusive);
                    
                    var elisionBuffer = CreateElisionBuffer(trackingViewSpan, _buffer);
                    var elisionBufferView = CreateElisionBufferView(elisionBuffer);

                    var hover = regionSpan.GetText();
                    yield return new TagSpan<IOutliningRegionTag>(
                        regionSpan,
                        new OutliningRegionTag(false, false, ellipsis, elisionBufferView));
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

        void CreateOutlineRegions()
        {
            //IList<SLang.DECLARATION> decls = ASTUtilities.GetUnitsAndStandalones(_buffer);

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
