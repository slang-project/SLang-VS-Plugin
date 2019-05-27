using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLangPlugin.Classification
{
    class ClassificationType
    {
        /// <summary>
        /// Defines the "Verilog_always" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("unit")]
        internal static ClassificationTypeDefinition SLangUnitType = null;
    }
}
