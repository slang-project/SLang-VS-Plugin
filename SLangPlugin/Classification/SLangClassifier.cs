using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Language.StandardClassification;

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

    [Export(typeof(IClassifierProvider))]
    [ContentType("SLang")]
    internal sealed class SLangClassifierProvider : IClassifierProvider
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
        internal IClassificationTypeRegistryService classificationTypeRegistry = null; // Set via MEF

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty<SLangClassifier>( creator: () => new SLangClassifier(this.classificationTypeRegistry) );
        }

    }

    internal sealed class SLangClassifier : IClassifier
    {

        private static readonly IList<ClassificationSpan> emptyList = new List<ClassificationSpan>(capacity: 0).AsReadOnly();

        IDictionary<SLangTokenType, IClassificationType> _SLangTypes;
        private IList<ClassificationSpan> lastSpans = emptyList;
        private ITextSnapshot lastSnapshot;
        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal SLangClassifier(IClassificationTypeRegistryService typeService)
        {
            _SLangTypes = new Dictionary<SLangTokenType, IClassificationType>();
            _SLangTypes[SLangTokenType.Identifier] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
            _SLangTypes[SLangTokenType.Keyword] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            _SLangTypes[SLangTokenType.Comment] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _SLangTypes[SLangTokenType.Operator] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Operator);
            _SLangTypes[SLangTokenType.StringLiteral] = typeService.GetClassificationType(PredefinedClassificationTypeNames.String);
            _SLangTypes[SLangTokenType.NumericLiteral] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Number);
            _SLangTypes[SLangTokenType.Other] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Other);
            _SLangTypes[SLangTokenType.Ignore] = typeService.GetClassificationType(PredefinedClassificationTypeNames.Other); // should not be accessed anyway
            _SLangTypes[SLangTokenType.Whitespace] = typeService.GetClassificationType(PredefinedClassificationTypeNames.WhiteSpace);

        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var snapshot = span.Snapshot;
            if (this.lastSnapshot == snapshot)
                return lastSpans;

            this.lastSnapshot = snapshot;
            
            return GetClassificationSpansAsync(snapshot).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<IList<ClassificationSpan>> GetClassificationSpansAsync(ITextSnapshot span)
        {
            //List<ClassificationSpan> tags = null;            
            //Task tagger = new Task(() => { tags = GetTags(span).ToList(); });
            //tagger.Start();
            //await Task.WhenAll(tagger);
            //return tags;

            //FIXME: async operation
            //List<ClassificationSpan> tags = null;
            //await new Task(() => { tags = GetTags(span).ToList(); });
            //return tags;

            return GetTags(span).ToList();
        }

        public IEnumerable<ClassificationSpan> GetTags(ITextSnapshot snapshot)
        {
            SLang.Reader reader = new SLang.Reader(snapshot.GetText());
            SLang.Tokenizer tokenizer = new SLang.Tokenizer(reader, (SLang.Options)null);

            SLang.Token token = tokenizer.getNextToken();

            while (token.code != SLang.TokenCode.EOS)
            {
                SLangTokenType tokenType = ClassificationMapping.getTokenType(token.code);
                if (tokenType != SLangTokenType.Ignore)
                {
                    Span currentTokenSpan = ConvertToSpan(token.span, snapshot);
                    SnapshotSpan tokenSpan = new SnapshotSpan(snapshot, currentTokenSpan);
                    yield return new ClassificationSpan(tokenSpan, _SLangTypes[tokenType]);
                }
                token = tokenizer.getNextToken();
            }
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
