using System;
using Windows.ApplicationModel.Resources;

namespace WinSteroid.Common.Helpers
{
    public static class ResourcesHelper
    {
        private static ResourceLoader _resourceLoader;
        private static ResourceLoader ResourceLoader
        {
            get
            {
                if (_resourceLoader == null)
                {
                    var resourceLoader = ResourceLoader.GetForCurrentView();
                    _resourceLoader = resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));
                }

                return _resourceLoader;
            }
        }

        public static string GetLocalizedString(string resourceKey)
        {
            return ResourceLoader.GetString(resourceKey);
        }
    }
}
