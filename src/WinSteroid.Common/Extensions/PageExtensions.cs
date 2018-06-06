using System;

namespace Windows.UI.Xaml.Controls
{
    public static class PageExtensions
    {
        public static string GetWidthStateName(this Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            switch (page.ActualWidth)
            {
                case var width when width >= 1000:
                    return "Wide";
                case var width when width >= 500 && width < 1000:
                    return "Normal";
                case var width when width >= 0 && width < 500:
                default:
                    return "Narrow";
            }
        }
    }
}
