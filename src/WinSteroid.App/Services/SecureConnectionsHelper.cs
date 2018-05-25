using Renci.SshNet;

namespace WinSteroid.Common.Helpers
{
    public static class SecureConnectionsHelper
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
