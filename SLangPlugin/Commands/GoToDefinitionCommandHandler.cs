using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;

namespace SLangPlugin.Commands
{
    class GoToDefinitionCommandHandler
    {
        readonly IServiceProvider _serviceProvider;
        readonly ITextView _textView;
        readonly ITextBuffer _textBuffer;

        SLangTokenTagger _generalTagger;

        public GoToDefinitionCommandHandler(IServiceProvider serviceProvider, ITextView textView, SLangTokenTagger generalTagger)
        {
            _serviceProvider = serviceProvider;
            _textView = textView;
            _textBuffer = textView.TextBuffer;
            _generalTagger = generalTagger;
        }

        public void showInfoMessage(string message)
        {
            VsShellUtilities.ShowMessageBox(
                            _serviceProvider,
                            message,
                            "Go To Definition Results",
                            OLEMSGICON.OLEMSGICON_INFO,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        private void FindSymbolInProject(string unitname)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            //DTE2 dte = _serviceProvider.GetService(typeof(DTE)) as DTE2;
            //var projects = dte.ActiveSolutionProjects;
            var currentPath = _textBuffer.GetTextDocument().FilePath;

            IList<SLang.DECLARATION> decls = ASTUtilities.GetUnitsAndStandalones(_textBuffer);
            foreach(var decl in decls)
            {
                if (decl.name.identifier.Equals(unitname))
                {
                    NavigateTo(currentPath, decl.span.begin.line - 1, decl.span.begin.pos - 2);
                    return;
                }
            }
            showInfoMessage("Definition Not Found.");
            
        }

        public void PerformSearch(ITextBuffer textBuffer, SnapshotPoint point) // make bool to check and show message in caller
        {
            foreach (var tag in _generalTagger._currentTags)
            {
                if (tag.Span.Start <= point && tag.Span.End > point)
                {
                    FindSymbolInProject(tag.Tag.token.image);
                    return;
                }
            }
        }

        private void NavigateTo(string targetFilepath, int targetLine, int targetColumn)
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
