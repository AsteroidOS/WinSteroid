using Windows.Foundation;

namespace Windows.UI.Xaml
{
    public static class FrameworkElementExtensions
    {
        public static Rect GetPickerRect(this FrameworkElement element)
        {
            var generalTransform = element.TransformToVisual(null);

            var point = generalTransform.TransformPoint(new Point());

            return new Rect(point, new Point(point.X + element.ActualWidth, point.Y + element.ActualHeight));
        }
    }
}
