using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin.Outliner
{
    // this is code is based on the Walkthrough: Outlining  (Implementing a Tagger Provider)
    // See: https://docs.microsoft.com/en-us/visualstudio/extensibility/walkthrough-outlining?view=vs-2015

    // Step #1: Create a class named OutliningTaggerProvider that implements ITaggerProvider, 
    // and export it with the ContentType and TagType attributes.
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(Constants.ContentType)]
    internal sealed class OutliningTaggerProvider : ITaggerProvider
    {
        // Step #2: Implement the CreateTagger method by adding an OutliningTagger to the properties of the buffer.
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            //create a single tagger for each buffer.
            Func<ITagger<T>> sc = delegate () { return new OutliningTagger(buffer) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(sc);
        }
    }
}
