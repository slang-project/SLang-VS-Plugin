using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin.Commands
{
    class FindAllReferencesCommandHandler
    {
        IServiceProvider _serviceProvider;
        ITextView _textView;
        SLangTokenTagger _generalTagger;
        public FindAllReferencesCommandHandler(IServiceProvider serviceProvider, ITextView textView, SLangTokenTagger generalTagger)
        {
            _serviceProvider = serviceProvider;
            _textView = textView;
            _generalTagger = generalTagger;
        }

        public void ShowInFindResultWindow( Location[] locations /*FileModel fileModel, NSpan span, Location[] locations*/)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (locations.Length == 0)
                return;

            //if (locations.Length == 1)
            //{
            //    GoToLocation(fileModel, locations[0]);
            //    return;
            //}

            var findSvc = (IVsFindSymbol)_serviceProvider.GetService(typeof(SVsObjectSearch));
            Debug.Assert(findSvc != null);

            var caption = "Test Caption";

            var libSearchResults = new LibraryNode("Ref Node", LibraryNode.LibraryNodeType.References, LibraryNode.LibraryNodeCapabilities.None, null);

            foreach (var location in locations)
            {
                var inner = new GoToInfoLibraryNode(location, caption, _serviceProvider);
                libSearchResults.AddNode(inner);
            }

            var package = SLangPluginPackage.Instance;
            package.SetFindResult(libSearchResults);
            var criteria =
              new[]
              {
                  new VSOBSEARCHCRITERIA2
                  {
                    eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD,
                    grfOptions = (uint)_VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
                    szName = "<dummy>",
                    dwCustom = Library.FindAllReferencesMagicNum,
                  }
              };

            var scope = Library.MagicGuid;
            findSvc.DoSearch(ref scope, criteria);
        }
    }
}
