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

using Renci.SshNet.Messages.Transport;
using System;

namespace Renci.SshNet
{
    public static class SshClientRebootExtensions
    {
        public static bool Reboot(this SshClient sshClient)
        {
            if (sshClient == null)
            {
                throw new ArgumentNullException(nameof(sshClient));
            }

            try
            {
                if (!sshClient.IsConnected)
                {
                    sshClient.Connect();
                }

                var command = sshClient.RunCommand("reboot");

                return command.ExitStatus == 0;
            }
            catch (Common.SshConnectionException sshException)
            {
                if (sshException.DisconnectReason == DisconnectReason.ConnectionLost)
                {
                    return true;
                }

                Microsoft.HockeyApp.HockeyClient.Current.TrackException(sshException);

                return false;
            }
            catch (Exception ex)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);

                return false;
            }
        }

        public static bool RebootAndEnterBootloader(this SshClient sshClient)
        {
            if (sshClient == null)
            {
                throw new ArgumentNullException(nameof(sshClient));
            }

            try
            {
                if (!sshClient.IsConnected)
                {
                    sshClient.Connect();
                }

                var command = sshClient.RunCommand("systemctl --force reboot bootloader");

                return command.ExitStatus == 0;
            }
            catch (Common.SshConnectionException sshException)
            {
                if (sshException.DisconnectReason == DisconnectReason.ConnectionLost)
                {
                    return true;
                }

                Microsoft.HockeyApp.HockeyClient.Current.TrackException(sshException);

                return false;
            }
            catch (Exception ex)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);

                return false;
            }
        }

        public static bool RebootAndEnterRecovery(this SshClient sshClient)
        {
            if (sshClient == null)
            {
                throw new ArgumentNullException(nameof(sshClient));
            }

            try
            {
                if (!sshClient.IsConnected)
                {
                    sshClient.Connect();
                }

                var command = sshClient.RunCommand("systemctl --force reboot recovery");

                return command.ExitStatus == 0;
            }
            catch (Common.SshConnectionException sshException)
            {
                if (sshException.DisconnectReason == DisconnectReason.ConnectionLost)
                {
                    return true;
                }

                Microsoft.HockeyApp.HockeyClient.Current.TrackException(sshException);

                return false;
            }
            catch (Exception ex)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);

                return false;
            }
        }
    }
}
