using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin.ErrorDetection
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(Constants.ContentType)]
    [TagType(typeof(ErrorTag))]
    internal sealed class SquigglesTaggerProvider : IViewTaggerProvider
    {
        [Import]
        private IBufferTagAggregatorFactoryService _aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            ITagger<T> sc()
            {
                return new SquigglesTagger(buffer, this._aggregatorFactory) as ITagger<T>;
            }
            return buffer.Properties.GetOrCreateSingletonProperty(sc);
        }
    }

    internal sealed class SquigglesTagger : ITagger<IErrorTag>
    {
        private readonly ITextBuffer _sourceBuffer;
        private readonly ITagAggregator<Classification.SLangTokenTag> _aggregator;

        internal SquigglesTagger(ITextBuffer buffer, IBufferTagAggregatorFactoryService aggregatorFactory)
        {
            this._sourceBuffer = buffer;
            ITagAggregator<Classification.SLangTokenTag> sc()
            {
                return aggregatorFactory.CreateTagAggregator<Classification.SLangTokenTag>(buffer);
            }
            this._aggregator = buffer.Properties.GetOrCreateSingletonProperty(sc);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (IMappingTagSpan<Classification.SLangTokenTag> myTokenTag in this._aggregator.GetTags(spans))
            {
                if (myTokenTag.Tag.type == Classification.SLangTokenType.NumericLiteral)
                {
                    SnapshotSpan tagSpan = myTokenTag.Span.GetSpans(_sourceBuffer)[0];
                    yield return new TagSpan<IErrorTag>(tagSpan, new ErrorTag(PredefinedErrorTypeNames.SyntaxError, "Don't use numbers here!"));
                }
            }
        }
    }
}
