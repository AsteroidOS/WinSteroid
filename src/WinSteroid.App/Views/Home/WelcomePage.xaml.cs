﻿//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
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

using Windows.UI.Xaml.Controls;
using WinSteroid.App.ViewModels.Home;

namespace WinSteroid.App.Views.Home
{
    public sealed partial class WelcomePage : Page
    {
        public WelcomePageViewModel ViewModel
        {
            get { return this.DataContext as WelcomePageViewModel; }
        }

        public WelcomePage()
        {
            this.InitializeComponent();
        }
    }
}
