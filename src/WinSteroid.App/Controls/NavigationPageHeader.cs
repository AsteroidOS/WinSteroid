using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinSteroid.App.Controls
{
    public class NavigationPageHeader : Control
    {
        public NavigationPageHeader()
        {
            this.DefaultStyleKey = typeof(NavigationPageHeader);
        }

        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }
        
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(NavigationPageHeader), new PropertyMetadata(string.Empty));

        public string ButtonGlyph
        {
            get { return (string)GetValue(ButtonGlyphProperty); }
            set { SetValue(ButtonGlyphProperty, value); }
        }

        public static readonly DependencyProperty ButtonGlyphProperty =
            DependencyProperty.Register(nameof(ButtonGlyph), typeof(string), typeof(NavigationPageHeader), new PropertyMetadata(null));


        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register(nameof(ButtonCommand), typeof(ICommand), typeof(NavigationPageHeader), new PropertyMetadata(null));

    }
}
