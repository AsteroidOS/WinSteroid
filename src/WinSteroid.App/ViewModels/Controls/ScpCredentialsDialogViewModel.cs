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

using GalaSoft.MvvmLight.Command;
using System.Net;
using System.Text;
using WinSteroid.Common.Helpers;
using WinSteroid.Shared;

namespace WinSteroid.App.ViewModels.Controls
{
    public class ScpCredentialsDialogViewModel : BaseViewModel
    {
        public override void Initialize() => this.Reset();

        public override void Reset()
        {
            this.HostIP = string.Empty;
            this.Username = string.Empty;
            this.Password = string.Empty;
            this.ValidationSummary = string.Empty;
        }

        private string _hostIP;
        public string HostIP
        {
            get { return _hostIP; }
            set { Set(nameof(HostIP), ref _hostIP, value); }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { Set(nameof(Username), ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { Set(nameof(Password), ref _password, value); }
        }

        private string _validationSummary;
        public string ValidationSummary
        {
            get { return _validationSummary; }
            set { Set(nameof(ValidationSummary), ref _validationSummary, value); }
        }

        private RelayCommand _defaultCredentialsCommand;
        public RelayCommand DefaultCredentialsCommand
        {
            get
            {
                if (_defaultCredentialsCommand == null)
                {
                    _defaultCredentialsCommand = new RelayCommand(InsertDefaultCredentials);
                }

                return _defaultCredentialsCommand;
            }
        }

        private void InsertDefaultCredentials()
        {
            this.HostIP = Asteroid.DefaultIPv4;
            this.Username = Asteroid.DefaultRootUsername;
            this.Password = string.Empty;
        }

        public bool Validate()
        {
            var validHostIP = IPAddress.TryParse(this.HostIP, out IPAddress ipAddress);
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

            return validForm;
        }
    }
}
