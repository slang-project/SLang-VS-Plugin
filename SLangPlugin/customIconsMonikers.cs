using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace SLangPlugin
{
    public static class customIconsMonikers
    {
        private static readonly Guid ManifestGuid = new Guid("61fac2ec-24eb-4103-9db8-59e4c1ef7c14");

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
