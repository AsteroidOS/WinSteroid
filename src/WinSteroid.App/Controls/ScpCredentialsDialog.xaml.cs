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

using System.Text;
using Windows.UI.Xaml.Controls;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.Controls
{
    public sealed partial class ScpCredentialsDialog : ContentDialog
    {
        public string HostIP { get; set; }

        public string Username { get; set; }
        
        public string Password { get; set; }

        public bool RememberCredentials { get; set; }

        public string ValidationSummary { get; set; }

        public ScpCredentialsDialog()
        {
            this.HostIP = string.Empty;
            this.Username = string.Empty;
            this.Password = string.Empty;
            this.ValidationSummary = string.Empty;
            this.InitializeComponent();
        }

        private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var validHostIP = IPHelper.IsValid(this.HostIP);
            var validUsername = !string.IsNullOrWhiteSpace(this.Username);
            var validPassword = true;

            var validForm = validHostIP && validUsername && validPassword;
            if (!validForm)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(ResourcesHelper.GetLocalizedString("ScpCredentialsDialogInvalidFieldsFirstSegmentMessage"));

                if (!validHostIP)
                {
                    stringBuilder.AppendLine("- Host / IP");
                }

                if (!validUsername)
                {
                    stringBuilder.AppendLine("- Username");
                }

                if (!validPassword)
                {
                    stringBuilder.AppendLine("- Password");
                }

                this.ValidationSummary = stringBuilder.ToString();
            }
            else
            {
                this.ValidationSummary = string.Empty;
            }

            if (validForm && this.RememberCredentials)
            {
                SettingsHelper.SaveScpCredentialsIntoVault(this.HostIP, this.Username, this.Password, overwriteIfExists: true);
            }

            args.Cancel = !validForm;
        }

        private void OnHostIpTextChanged(object sender, TextChangedEventArgs e)
        {
            var passwordCredential = SettingsHelper.GetScpCredentialsFromVault(this.HostIP);
            if (passwordCredential == null) return;

            this.Username = passwordCredential.UserName;
            this.Password = passwordCredential.Password;
        }
    }
}
