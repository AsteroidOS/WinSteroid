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

using Renci.SshNet;

namespace WinSteroid.App.Factory
{
    public static class SecureConnectionsFactory
    {        
        public static ScpClient CreateScpClient(string ip, string username, string password)
        {
            var authenticationsMethods = new AuthenticationMethod[]
            {
                new NoneAuthenticationMethod(username)
            };

            var connectionInfo = new ConnectionInfo(ip, 22, username, authenticationsMethods);

            var client = new ScpClient(connectionInfo);

            if (client.IsConnected) return client;

            client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
            client.Connect();

            return client;
        }

        public static SshClient CreateSshClient(string ip, string username, string password)
        {
            var authenticationsMethods = new AuthenticationMethod[]
            {
                new NoneAuthenticationMethod(username)
            };

            var connectionInfo = new ConnectionInfo(ip, username, authenticationsMethods);

            var client = new SshClient(connectionInfo);

            if (client.IsConnected) return client;
            
            client.Connect();

            return client;
        }
    }
}
