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

using System.Linq;
using Windows.Security.Credentials;
using Windows.Storage;

namespace WinSteroid.Common.Helpers
{
    public static class SettingsHelper
    {
        private static PasswordVault _passwordVault;
        private static PasswordVault PasswordVault
        {
            get
            {
                if (_passwordVault == null)
                {
                    _passwordVault = new PasswordVault();
                }

                return _passwordVault;
            }
        }

        public static T GetValue<T>(string key, T defaultValue)
        {
            var result = ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out object value);
            if (result && value is T desiredValue)
            {
                return desiredValue;
            }

            return defaultValue;
        }

        public static void SetValue<T>(string key, T value)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }

        public static PasswordCredential GetScpCredentialsFromVault(string hostIp)
        {
            var passwordCredentials = PasswordVault.RetrieveAll();

            return passwordCredentials.SingleOrDefault(passwordCredential => passwordCredential.Resource == hostIp);
        }

        public static bool SaveScpCredentialsIntoVault(string hostIp, string userName, string password, bool overwriteIfExists)
        {
            var existingPasswordCredential = GetScpCredentialsFromVault(hostIp);
            if (existingPasswordCredential != null)
            {
                if (!overwriteIfExists) return false;

                PasswordVault.Remove(existingPasswordCredential);
            }

            PasswordVault.Add(new PasswordCredential(hostIp, userName, password));

            return true;
        }

        public static void RemoveAllScpCredentials()
        {
            var passwordCredentials = PasswordVault.RetrieveAll();
            if (passwordCredentials.IsNullOrEmpty()) return;

            foreach (var passwordCredential in passwordCredentials)
            {
                PasswordVault.Remove(passwordCredential);
            }
        }
    }
}
