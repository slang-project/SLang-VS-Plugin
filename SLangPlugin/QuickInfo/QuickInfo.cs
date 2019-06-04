using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SLangPlugin.QuickInfo
{
    #region QuickInfo Provider

    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("SLang Quick Info Provider")]
    [ContentType(Constants.ContentType)]
    [Order]
    internal sealed class SLangQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            // This ensures only one instance per textbuffer is created
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new SLangQuickInfoSource(textBuffer));
        }
    }
    #endregion

    #region QuickInfo

    internal sealed class SLangQuickInfoSource : IAsyncQuickInfoSource
    {
        private static readonly ImageId _icon1 = KnownMonikers.AbstractCube.ToImageId();
        private static readonly ImageId _icon2 = KnownMonikers.AbsolutePosition.ToImageId();

        private ITextBuffer _textBuffer;

        public SLangQuickInfoSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);

            if (triggerPoint != null)
            {
                var line = triggerPoint.Value.GetContainingLine();
                var lineNumber = triggerPoint.Value.GetContainingLine().LineNumber;
                var lineOffset = triggerPoint.Value.Position - line.Start.Position;
                

                var lineSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeInclusive);

                var lineNumberElm = new ContainerElement(
                    ContainerElementStyle.Wrapped,
                    new ImageElement(_icon1),
                    new ClassifiedTextElement(
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "Line number: "),
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, $"{lineNumber + 1}")
                    ));
                var linePosElm = new ContainerElement(
                    ContainerElementStyle.Wrapped,
                    new ImageElement(_icon2),
                    new ClassifiedTextElement(
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "Offset: "),
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, $"{lineOffset + 1}")
                    ));

                var resultElm = new ContainerElement(
                    ContainerElementStyle.Stacked,
                    lineNumberElm,
                    linePosElm
                );

                return Task.FromResult(new QuickInfoItem(lineSpan, resultElm));
            }

            return Task.FromResult<QuickInfoItem>(null);
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }
    }
    #endregion
}
