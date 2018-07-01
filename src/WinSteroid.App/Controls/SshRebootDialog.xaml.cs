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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinSteroid.App.Controls
{
    public sealed partial class SshRebootDialog : ContentDialog
    {
        public RebootMode SelectedRebootMode { get; private set; }

        public SshRebootDialog()
        {
            this.InitializeComponent();
        }

        public enum RebootMode
        {
            None = -1,
            Classic = 0,
            Bootloader = 1,
            Recovery = 2
        }

        private void OnClassicModeClicked(object sender, RoutedEventArgs e)
        {
            this.SelectedRebootMode = RebootMode.Classic;
            this.Hide();
        }

        private void OnBootloaderModeClicked(object sender, RoutedEventArgs e)
        {
            this.SelectedRebootMode = RebootMode.Bootloader;
            this.Hide();
        }

        private void OnRecoveryModeClicked(object sender, RoutedEventArgs e)
        {
            this.SelectedRebootMode = RebootMode.Recovery;
            this.Hide();
        }
    }
}
