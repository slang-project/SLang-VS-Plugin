using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

// based on https://github.com/rsdn/nitra/

namespace SLangPlugin
{
    class Location
    {
        // TODO: Just for testing, refactor later
        public string filepath;
        public int line;
        public int column;

        public Location(string filepath, int line, int column)
        {
            this.filepath = filepath;
            this.line = line;
            this.column = column;
        }
    }

    class GoToInfoLibraryNode : LibraryNode
    {
        private readonly Location _location;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _caption;
        private readonly string Text;

        public GoToInfoLibraryNode(Location location, string caption, IServiceProvider serviceProvider)
            : base(caption)
        {
            _location = location;
            _serviceProvider = serviceProvider;
            _caption = caption;
            CanGoToSource = true;
        }

        public string Path => _location.filepath;

        protected override void GotoSource(Microsoft.VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE gotoType)
        {
            NavigateTo(_location.filepath, _location.line, _location.column);
        }

        protected override uint CategoryField(LIB_CATEGORY category)
        {
            return (uint)LibraryNodeType.None;
        }

        protected override string GetTextWithOwnership(VSTREETEXTOPTIONS tto)
        {
            if (tto == VSTREETEXTOPTIONS.TTO_DEFAULT)
            {
                // TODO: don't use just text, make interactive tile
                return $"{Name} {Path} - ({_location.line}, {_location.column}) : {Text}";
            }

            return null;
        }

        private void NavigateTo(string targetFilepath, int targetLine, int targetColumn)
        {
            // TODO: consider if relevant

            //string sourceFilepath = _textBuffer.GetTextDocument().FilePath;
            if (false/*sourceFilepath == targetFilepath*/)
            {
                // Current File
                //ITextSnapshotLine newLine = _textBuffer.CurrentSnapshot.Lines.ElementAt(targetLine);
                //_textView.Caret.MoveTo(newLine.Start.Add(targetColumn));
                //_textView.ViewScroller.EnsureSpanVisible(_textView.GetTextElementSpan(newLine.Start));
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
