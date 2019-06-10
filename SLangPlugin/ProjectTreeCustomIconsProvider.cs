using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Reflection;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem;

namespace SLangPlugin
{
    /// <summary>
    /// Updates nodes in the project tree by overriding property values calcuated so far by lower priority providers.
    /// </summary>
    [Export(typeof(IProjectTreePropertiesProvider))]
    [AppliesTo(MyUnconfiguredProject.UniqueCapability)]
    // TODO: Consider removing the Order attribute as it typically should not be needed when creating a new project type. It may be needed when customizing an existing project type to override the default behavior (e.g. the default C# implementation).
    //[Order(1000)]
    internal class ProjectTreeCustomIconsProvider : IProjectTreePropertiesProvider
    {
        const string ITEM_TYPE = "SLangCompile";

        /// <summary>
        /// Calculates new property values for each node in the project tree.
        /// </summary>
        /// <param name="propertyContext">Context information that can be used for the calculation.</param>
        /// <param name="propertyValues">Values calculated so far for the current node by lower priority tree properties providers.</param>
        public void CalculatePropertyValues(
            IProjectTreeCustomizablePropertyContext propertyContext,
            IProjectTreeCustomizablePropertyValues propertyValues)
        {
            if (propertyValues.Flags.Contains(ProjectTreeFlags.Common.ProjectRoot))
            {
                propertyValues.Icon = customIconsMonikers.ProjectIconImageMoniker.ToProjectSystemType();
            }
            else if (propertyContext.ItemType != null ? propertyContext.ItemType.Equals(ITEM_TYPE) : false)
            {
                propertyValues.Icon = customIconsMonikers.ItemIconImageMoniker.ToProjectSystemType();
            }


        }
    }
}