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
using System.Collections.Generic;
using System.Linq;
using WinSteroid.Common.Models;
using WinSteroid.Common.SshCommands;

namespace Renci.SshNet
{
    public static class SshClientUsbModeExtensions
    {
        public static List<UsbMode> GetUsbModes(this SshClient sshClient)
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

                var command = sshClient.RunCommand("usb_moded_util -m");

                return UsbModeHelper.ParseModes(command.Result).ToList();
            }
            catch (Exception ex)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);

                return new List<UsbMode>();
            }
        }

        public static UsbMode GetCurrentUsbMode(this SshClient sshClient)
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

                var command = sshClient.RunCommand("usb_moded_util -q");

                return UsbModeHelper.Parse(command.Result);
            }
            catch (Exception ex)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);

                return null;
            }
        }

        public static bool SetUsbMode(this SshClient sshClient, string mode)
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

                var command = sshClient.RunCommand("usb_moded_util -s " + mode);

                return command.ExitStatus == 0;
            }
            catch (Exception ex)
            {
                Microsoft.HockeyApp.HockeyClient.Current.TrackException(ex);

                return false;
            }
        }
    }
}
