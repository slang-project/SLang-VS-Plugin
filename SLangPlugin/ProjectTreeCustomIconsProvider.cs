using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Reflection;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem;

namespace SLangPlugin
{
    /// <summary>
    /// Update nodes in the project tree by overriding property values calcuated so far by lower priority providers.
    /// </summary>
    [Export(typeof(IProjectTreePropertiesProvider))]
    [AppliesTo(MyUnconfiguredProject.UniqueCapability)]
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