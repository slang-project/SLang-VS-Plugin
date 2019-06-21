using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin.Commands
{
    class GoToDefinitionCommandHandler
    {
        readonly IServiceProvider _serviceProvider;
        readonly ITextView _textView;
        readonly ITextBuffer _textBuffer;

        public GoToDefinitionCommandHandler(IServiceProvider serviceProvider, ITextView textView)
        {
            _serviceProvider = serviceProvider;
            _textView = textView;
            _textBuffer = textView.TextBuffer;
        }

        public void NavigateTo(string targetFilepath, int targetLine, int targetColumn)
        {
            string sourceFilepath = _textBuffer.GetTextDocument().FilePath;
            if (sourceFilepath == targetFilepath)
            {
                // Current File
                ITextSnapshotLine newLine = _textBuffer.CurrentSnapshot.Lines.ElementAt(targetLine);
                _textView.Caret.MoveTo(newLine.Start.Add(targetColumn));
                _textView.ViewScroller.EnsureSpanVisible(_textView.GetTextElementSpan(newLine.Start));
            }
            else
            {
                // Other File
                VsUtilities.NavigateTo(_serviceProvider,
                                       targetFilepath,
                                       Guid.Empty,
                                       targetLine,
                                       targetColumn);
            }
        }
    }
}
