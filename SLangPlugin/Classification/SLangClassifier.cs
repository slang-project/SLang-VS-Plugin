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

    #region Tagger Provider

    [Export(typeof(ITaggerProvider))]
    [ContentType(Constants.ContentType)]
    [TagType(typeof(ClassificationTag))]
    internal sealed class SLangClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name(Constants.ContentType)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition SLangContentType = null;

        [Export]
        [FileExtension(".slang")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition SLangFileType = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        [Import]
        internal IStandardClassificationService standardClassificationService = null;

        [Import]
        internal IClassificationTypeRegistryService classificationTypeRegistryService = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ITagAggregator<SLangTokenTag> SLangTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<SLangTokenTag>(buffer);

            Func<ITagger<T>> creator = () => { return new SLangClassifier(buffer, SLangTagAggregator,
                standardClassificationService, classificationTypeRegistryService) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(creator);
        }
    }
    #endregion

    #region Tagger

    internal sealed class SLangClassifier : ITagger<ClassificationTag>
    {

        ITextBuffer _buffer;
        //ITextSnapshot _snapshot;
        ITagAggregator<SLangTokenTag> _aggregator;
        IDictionary<SLangTokenType, IClassificationType> _SLangTypes;

        internal SLangClassifier(ITextBuffer buffer, ITagAggregator<SLangTokenTag> SLangTagAggregator,
            IStandardClassificationService typeService, IClassificationTypeRegistryService typeRegistry)
        {
            _buffer = buffer;
            //_snapshot = buffer.CurrentSnapshot;
            _aggregator = SLangTagAggregator;
            InitializeClassifierMapping(typeService, typeRegistry);
            buffer.Changed += BufferChanged;
        }

        void InitializeClassifierMapping(IStandardClassificationService typeService, IClassificationTypeRegistryService typeRegistry)
        {
            _SLangTypes = new Dictionary<SLangTokenType, IClassificationType>();

            _SLangTypes[SLangTokenType.Identifier] = typeService.SymbolDefinition;
            _SLangTypes[SLangTokenType.Keyword] = typeService.Keyword;
            _SLangTypes[SLangTokenType.Comment] = typeService.Comment;
            _SLangTypes[SLangTokenType.Operator] = typeService.Operator;
            _SLangTypes[SLangTokenType.StringLiteral] = typeService.StringLiteral;
            _SLangTypes[SLangTokenType.NumericLiteral] = typeService.NumberLiteral;
            _SLangTypes[SLangTokenType.Other] = typeService.Other;
            _SLangTypes[SLangTokenType.Ignore] = typeService.Other;
            _SLangTypes[SLangTokenType.Whitespace] = typeService.WhiteSpace;

            _SLangTypes[SLangTokenType.Unit] = typeRegistry.GetClassificationType("unit");
        }

        public void BufferChanged(object obj, TextContentChangedEventArgs args)
        {
            //FIXME: Does not work for deleting comment symbols, maybe some other (semantic) considerations should be counted

            //bool containsSpanChange = false;
            //foreach (var change in args.Changes)
            //{
                
            //    if (change.NewText.Contains("*") || change.NewText.Contains("/"))
            //    {
            //        containsSpanChange = true;
            //        break;
            //    }
            //}
            //if (!containsSpanChange)
            //    return;


            // if current change does not correspond to latest version of buffer, ignore until newer
            if (args.After != _buffer.CurrentSnapshot)
                return;

            TagsChanged?.Invoke(obj, new SnapshotSpanEventArgs(new SnapshotSpan(_buffer.CurrentSnapshot, 0,
                    _buffer.CurrentSnapshot.Length)));
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tagSpan in _aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                if (spans.IntersectsWith(tagSpans))
                    yield return new TagSpan<ClassificationTag>(tagSpans[0], new ClassificationTag(_SLangTypes[tagSpan.Tag.type]));
            }
        }
    }
    #endregion
}
