using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Language.StandardClassification;

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
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<SLangTokenTag> SLangTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<SLangTokenTag>(buffer);

            return new SLangClassifier(buffer, SLangTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class SLangClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<SLangTokenTag> _aggregator;
        IDictionary<SLangTokenType, IClassificationType> _SLangTypes;

        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal SLangClassifier(ITextBuffer buffer,
                               ITagAggregator<SLangTokenTag> SLangTagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = SLangTagAggregator;
            _SLangTypes = new Dictionary<SLangTokenType, IClassificationType>();
            _SLangTypes[SLangTokenType.Identifier] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
            _SLangTypes[SLangTokenType.Keyword] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            _SLangTypes[SLangTokenType.Comment] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _SLangTypes[SLangTokenType.Operator] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Operator);
            _SLangTypes[SLangTokenType.StringLiteral] = typeService.GetClassificationType(PredefinedClassificationTypeNames.String);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Search the given span for any instances of classified tags
        /// </summary>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tagSpan in _aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return
                    new TagSpan<ClassificationTag>(tagSpans[0],
                                                   new ClassificationTag(_SLangTypes[tagSpan.Tag.type]));
            }
        }
    }
}
