using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Language.StandardClassification;
using System.ComponentModel.Composition;

namespace SLangPlugin.Classification
{

    [Export(typeof(ITaggerProvider))]
    [ContentType("SLang")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class SLangClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("SLang")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition SLangContentType = null;

        [Export]
        [FileExtension(".slang")]
        [ContentType("SLang")]
        internal static FileExtensionToContentTypeDefinition SLangFileType = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        [Import]
        internal IStandardClassificationService classificationTypeRegistry = null; // Set via MEF

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<SLangTokenTag> SLangTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<SLangTokenTag>(buffer);

            return new SLangClassifier(buffer, SLangTagAggregator, classificationTypeRegistry) as ITagger<T>;
        }

        //public IClassifier GetClassifier(ITextBuffer textBuffer)
        //{
        //    return textBuffer.Properties.GetOrCreateSingletonProperty<SLangClassifier>( creator: () => new SLangClassifier(this.classificationTypeRegistry) );
        //}

    }

    internal sealed class SLangClassifier : ITagger<ClassificationTag>
    {

        IDictionary<SLangTokenType, IClassificationType> _SLangTypes;
        ITextSnapshot _snapshot;
        ITagAggregator<SLangTokenTag> _aggregator;
        ITextBuffer _buffer;

        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal SLangClassifier(ITextBuffer buffer,
                               ITagAggregator<SLangTokenTag> SLangTagAggregator, IStandardClassificationService typeService)
        {
            _buffer = buffer;
            _snapshot = buffer.CurrentSnapshot;
            _aggregator = SLangTagAggregator;
            InitializeClassifierMapping(typeService);
        }

        void InitializeClassifierMapping(IStandardClassificationService typeService)
        {
            _SLangTypes = new Dictionary<SLangTokenType, IClassificationType>();
            _SLangTypes[SLangTokenType.Identifier] = typeService.Identifier;
            _SLangTypes[SLangTokenType.Keyword] = typeService.Keyword;
            _SLangTypes[SLangTokenType.Comment] = typeService.Comment;
            _SLangTypes[SLangTokenType.Operator] = typeService.Operator;
            _SLangTypes[SLangTokenType.StringLiteral] = typeService.StringLiteral;
            _SLangTypes[SLangTokenType.NumericLiteral] = typeService.NumberLiteral;
            _SLangTypes[SLangTokenType.Other] = typeService.Other;
            _SLangTypes[SLangTokenType.Ignore] = typeService.Other;
            _SLangTypes[SLangTokenType.Whitespace] = typeService.WhiteSpace;

        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        //public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        //{
        //    var snapshot = span.Snapshot;
        //    if (this.lastSnapshot == snapshot)
        //        return lastSpans;

        //    this.lastSnapshot = snapshot;
            
        //    var res = GetClassificationSpansAsync(snapshot).ConfigureAwait(false).GetAwaiter().GetResult();
        //    return res;
        //}

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_snapshot.GetLineFromLineNumber(0).Start, _snapshot.GetLineFromLineNumber(_snapshot.LineCount-1).End)));
            foreach (var tagSpan in _aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return new TagSpan<ClassificationTag>(tagSpans[0], new ClassificationTag(_SLangTypes[tagSpan.Tag.type]));
            }
        }
    }
}
