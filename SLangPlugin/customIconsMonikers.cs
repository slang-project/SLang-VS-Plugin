using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace SLangPlugin
{
    public static class customIconsMonikers
    {
        private static readonly Guid ManifestGuid = new Guid("DE569D75-A6A3-421B-813C-802B35E6E00E");

        private const int ProjectIcon = 0;
        private const int ItemIcon = 1;

        public static ImageMoniker ProjectIconImageMoniker
        {
            get
            {
                return new ImageMoniker { Guid = ManifestGuid, Id = ProjectIcon };
            }
        }

        public static ImageMoniker ItemIconImageMoniker
        {
            get
            {
                return new ImageMoniker { Guid = ManifestGuid, Id = ItemIcon };
            }
        }
    }
}
