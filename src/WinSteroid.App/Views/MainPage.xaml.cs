//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.

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
                From = oldPercentage,
                To = newPercentage,
                Duration = new Duration(TimeSpan.FromMilliseconds(milliSeconds)),
                EnableDependentAnimation = true
            };

            Storyboard.SetTarget(doubleAnimation, PercentageRadialProgressBar);
            Storyboard.SetTargetProperty(doubleAnimation, nameof(RadialProgressBar.Value));

            storyBoard.Children.Add(doubleAnimation);
            storyBoard.Begin();
        }

        private void OnMenuOptionClick(object sender, ItemClickEventArgs e)
        {
            if (!(e.ClickedItem is MenuOptionViewModel menuOption)) return;

            this.ViewModel.ManageSelectedMenuOption(menuOption);
        }
    }
}
