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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Windows.ApplicationModel.AppService
{
    public static class AppServiceExtensions
    {
        public static IAsyncOperation<AppServiceResponseStatus> SendResponseAsync(this AppServiceRequestReceivedEventArgs args, ValueSet message)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                return args.Request.SendResponseAsync(message);
            }
            catch
            {
                return AsyncInfo.Run(ct => Task.FromResult(AppServiceResponseStatus.Failure));
            }
        }
    }
}
