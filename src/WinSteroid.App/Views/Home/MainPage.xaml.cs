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

using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.ViewModels;
using WinSteroid.App.ViewModels.Home;

namespace WinSteroid.App.Views.Home
{
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel
        {
            get { return this.DataContext as ViewModels.Home.MainPageViewModel; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void OnMenuOptionClick(object sender, ItemClickEventArgs e)
        {
            if (!(e.ClickedItem is MenuOptionViewModel menuOption)) return;

            this.ViewModel.ManageSelectedMenuOption(menuOption);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.ViewModel.UpdateNotificationsOptions();

            if (this.Content is Grid grid)
            {
                grid.UpdateLayout();
            }

            base.OnNavigatedTo(e);
        }

        private async void OnNotificationClick(object sender, ItemClickEventArgs e)
        {
            if (!(e.ClickedItem is NotificationItemViewModel notification)) return;

            if (notification.LaunchUri == null) return;
            
            var launcherOptions = new LauncherOptions
            {
                TreatAsUntrusted = false,
                TargetApplicationPackageFamilyName = notification.AppId
            };

            await Launcher.LaunchUriAsync(notification.LaunchUri, launcherOptions);
        }
    }
}
