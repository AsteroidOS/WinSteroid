using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.ViewModels;

namespace WinSteroid.App.Views
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        public static int Percentage { get; set; }

        public MainPageViewModel ViewModel
        {
            get { return this.DataContext as MainPageViewModel; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Current = this;
        }

        public void UpdatePercentage(int oldPercentage, int newPercentage)
        {
            var milliSeconds = Math.Abs(newPercentage - oldPercentage) * 10;
            if (milliSeconds < 100)
            {
                milliSeconds = 100;
            }

            var storyBoard = new Storyboard();

            var doubleAnimation = new DoubleAnimation()
            {
                To = newPercentage,
                Duration = new Duration(TimeSpan.FromMilliseconds(milliSeconds)),
                EnableDependentAnimation = true
            };

            Storyboard.SetTarget(doubleAnimation, PercentageRadialProgressBar);
            Storyboard.SetTargetProperty(doubleAnimation, nameof(RadialProgressBar.Value));

            storyBoard.Children.Add(doubleAnimation);
            storyBoard.Begin();
        }
    }
}
