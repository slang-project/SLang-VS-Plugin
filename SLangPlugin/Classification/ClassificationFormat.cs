using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SLangPlugin.Classification
{
    #region Classification Type
    class ClassificationType
    {
        /// <summary>
        /// Defines the "Unit" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("unit")]
        internal static ClassificationTypeDefinition SLangUnitType = null;
    }
    #endregion

    #region Classification Format
    class ClassificationFormat
    {
        /// <summary>
        /// Defines the editor format for the Verilog_always classification type. Text is colored BlueViolet
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "unit")]
        [Name("unit")]
        //this should be visible to the end user
        [UserVisible(true)] // sets this editor format definition visible for the user (in Tools>Options>Environment>Fonts and Colors>Text Editor
                            //set the priority to be after the default classifiers
        [Order(Before = Priority.Default)]
        internal sealed class SLangUnitType : ClassificationFormatDefinition
        {
            /// <summary>
            /// Defines the visual format for the "always" classification type
            /// </summary>
            public SLangUnitType()
            {
                DisplayName = "SLang::Unit";
                ForegroundColor = (Color)ColorConverter.ConvertFromString("#4ec9b0");

                // https://docs.microsoft.com/en-us/dotnet/api/system.attribute.getcustomattributes?view=netframework-4.7.2
                // System.Windows.Media.Colors mc = Microsoft.VisualStudio.Text.Classification.ClassificationTypeAttribute.GetCustomAttributes();
                // EditorFormatDefinition.DisplayName();
            }
        }
    }
    #endregion
}
