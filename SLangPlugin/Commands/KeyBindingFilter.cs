using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// based on https://github.com/microsoft/dafny/tree/master/Source/DafnyExtension

namespace SLangPlugin.Commands
{
    internal class KeyBindingCommandFilter : IOleCommandTarget
    {
        private IWpfTextView m_textView;
        internal IOleCommandTarget m_nextTarget;
        internal bool m_added;

        //[Import(typeof(Microsoft.VisualStudio.Shell.SVsServiceProvider))]
        internal System.IServiceProvider _serviceProvider = null;
        GoToDefinitionCommandHandler _goToDefinitionCommandHandler;

        public KeyBindingCommandFilter(IWpfTextView textView, System.IServiceProvider serviceProvider)
        {
            m_textView = textView;
            _serviceProvider = serviceProvider;
            _goToDefinitionCommandHandler = new GoToDefinitionCommandHandler(_serviceProvider, m_textView);
        }

        public void showInfoMessage(string message)
        {
            VsShellUtilities.ShowMessageBox(
                            _serviceProvider,
                            message,
                            nameof(KeyBindingCommandFilter),
                            OLEMSGICON.OLEMSGICON_INFO,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == typeof(VSConstants.VSStd97CmdID).GUID)
            {
                if (cCmds == 1)
                {
                    switch (prgCmds[0].cmdID)
                    {
                        case (uint)VSConstants.VSStd97CmdID.GotoDefn:
                        case (uint)VSConstants.VSStd12CmdID.PeekDefinition:
                        case (uint)VSConstants.VSStd97CmdID.FindReferences:
                        case (uint)VSConstants.VSStd97CmdID.F1Help:
                            prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                            prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == typeof(VSConstants.VSStd12CmdID).GUID)
            {
                if (cCmds == 1)
                {
                    switch (prgCmds[0].cmdID)
                    {
                        case (uint)VSConstants.VSStd12CmdID.PeekDefinition:
                            prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                            prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
                            return VSConstants.S_OK;
                    }
                }
            }
            else if (pguidCmdGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                if (cCmds == 1)
                {
                    switch (prgCmds[0].cmdID)
                    {
                        case (uint)VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                        case (uint)VSConstants.VSStd2KCmdID.FORMATSELECTION:
                            prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                            prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
                            return VSConstants.S_OK;
                    }
                }
            }
            return m_nextTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            SnapshotPoint caretPoint = m_textView.Caret.Position.BufferPosition;
            ITextSnapshotLine containingLine = caretPoint.GetContainingLine();
            int lineNumber = containingLine.LineNumber;
            int lineOffset = caretPoint.Position - containingLine.Start;

            ITextDocument textDocument = Utilities.GetTextDocument(m_textView.TextBuffer);
            string filename = (textDocument != null) ? textDocument.FilePath : ""; //TODO: optimize - move to constructor

            if (pguidCmdGroup == typeof(VSConstants.VSStd97CmdID).GUID)
            {
                switch (nCmdID)
                {
                    case (uint)VSConstants.VSStd97CmdID.FindReferences:
                        showInfoMessage($"FindAllReferences from symbol at line: {lineNumber}, offset:{lineOffset}");
                        // To consider https://github.com/Microsoft/PTVS/blob/master/Python/Product/PythonTools/PythonTools/Navigation/EditFilter.cs
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.F1Help:
                        // TODO: understand pointed token and construct appropriate url
                        System.Diagnostics.Process.Start("https://github.com/slang-project/SLang");
                        return VSConstants.S_OK;

                    case (uint)VSConstants.VSStd97CmdID.GotoDefn:
                        showInfoMessage($"GoToDefinition from symbol at line: {lineNumber}, offset:{lineOffset}");
                        _goToDefinitionCommandHandler.NavigateTo("C:\\Users\\Maksim Surkov\\source\\repos\\SLangProjectTemplate14\\App.slang", 1, 1); // TODO: replace with actual search call
                        return VSConstants.S_OK;
                }
            }
            else if (pguidCmdGroup == typeof(VSConstants.VSStd12CmdID).GUID)
            {
                switch (nCmdID)
                {
                    case (uint)VSConstants.VSStd12CmdID.PeekDefinition:
                        showInfoMessage($"PeekDefinition from symbol at line: {lineNumber}, offset:{lineOffset}");
                        return VSConstants.S_OK;
                }
            }
            else if (pguidCmdGroup == typeof(VSConstants.VSStd2KCmdID).GUID)
            {
                switch (nCmdID)
                {
                    case (uint)VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                        showInfoMessage($"FormatDocument for {filename}");
                        return VSConstants.S_OK;
                    case (uint)VSConstants.VSStd2KCmdID.FORMATSELECTION:
                        ITextSelection selection = m_textView.Selection;
                        if (!selection.IsEmpty)
                        {
                            ITextSnapshotLine selectionStartContainingLine = selection.Start.Position.GetContainingLine();
                            ITextSnapshotLine selectionEndContainingLine = selection.End.Position.GetContainingLine();

                            int selectionStartLineNumber = selectionStartContainingLine.LineNumber;
                            int selectionStartLineOffset = selection.Start.Position.Position - selectionStartContainingLine.Start;
                            int selectionEndLineNumber = selectionEndContainingLine.LineNumber;
                            int selectionEndLineOffset = selection.End.Position.Position - selectionEndContainingLine.Start;

                            showInfoMessage($"FormatSelection from (line: {selectionStartLineNumber}, offset:{selectionStartLineOffset}) to (line: {selectionEndLineNumber}, offset:{selectionEndLineOffset})");
                            return VSConstants.S_OK;
                        }
                        else
                        {
                            showInfoMessage($"You have not selected anything.");
                            return VSConstants.S_OK;
                        }
                }
            }

            return m_nextTarget.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }
    }
    

    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(Constants.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class KeyBindingCommandFilterProvider : IVsTextViewCreationListener
    {

        [Import(typeof(IVsEditorAdaptersFactoryService))]
        internal IVsEditorAdaptersFactoryService editorFactory = null;

        //// Must hold strong reference to both of the following in order for DocumentSaved to work
        //Events events;
        //EnvDTE.DocumentEvents documentEvents;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = editorFactory.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;


            // add Document saved event
            //DTE dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            //events = (Events)dte.Events;
            //documentEvents = events.DocumentEvents;
            //documentEvents.DocumentSaved += DocumentSaved;

            AddCommandFilter(textViewAdapter, new KeyBindingCommandFilter(textView, ServiceProvider.GlobalProvider));
        }

        private void DocumentSaved(EnvDTE.Document document)
        {
            //DafnyLanguage.ProgressTagger tagger;
            //IWpfTextView textView = GetWpfTextView(document.FullName);
            //if (textView != null && DafnyLanguage.ProgressTagger.ProgressTaggers.TryGetValue(textView.TextBuffer, out tagger))
            //{
            //    MenuProxy.Output("restart verifier on file save: " + document.FullName + "\n");
            //    // stop the old verification
            //    tagger.StopVerification();

            //    // start a new one.
            //    tagger.StartVerification(false);
            //}
        }

        // Maybe useful but currently unused

        //private IWpfTextView GetWpfTextView(string filePath)
        //{
        //    DTE dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
        //    Microsoft.VisualStudio.OLE.Interop.IServiceProvider provider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte;
        //    ServiceProvider serviceProvider = new ServiceProvider(provider);

        //    IVsUIHierarchy uiHierarchy;
        //    uint itemID;
        //    IVsWindowFrame windowFrame;

        //    if (Microsoft.VisualStudio.Shell.VsShellUtilities.IsDocumentOpen(serviceProvider, filePath, Guid.Empty,
        //                                    out uiHierarchy, out itemID, out windowFrame))
        //    {
        //        // Get the IVsTextView from the windowFrame.
        //        IVsTextView textView = Microsoft.VisualStudio.Shell.VsShellUtilities.GetTextView(windowFrame);
        //        return editorFactory.GetWpfTextView(textView);
        //    }

        //    return null;
        //}


        void AddCommandFilter(IVsTextView viewAdapter, KeyBindingCommandFilter commandFilter)
        {
            if (commandFilter.m_added == false)
            {
                //get the view adapter from the editor factory
                IOleCommandTarget next;
                int hr = viewAdapter.AddCommandFilter(commandFilter, out next);

                if (hr == VSConstants.S_OK)
                {
                    commandFilter.m_added = true;
                    //you'll need the next target for Exec and QueryStatus
                    if (next != null)
                        commandFilter.m_nextTarget = next;
                }
            }
        }

    }
}
