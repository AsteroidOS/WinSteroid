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
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Windows.ApplicationModel.AppService
{
    public static class AppServiceConnectionExtensions
    {
        public static async Task<string> SendMessageWithResponseAsync(this AppServiceConnection appServiceConnection, ValueSet message)
        {
            if (appServiceConnection == null)
            {
                throw new ArgumentNullException(nameof(appServiceConnection));
            }

            var appServiceResponse = await appServiceConnection.SendMessageAsync(message);
            if (appServiceResponse.Status != AppServiceResponseStatus.Success)
            {
                return "Operation failed";
            }

            var responseMessage = appServiceResponse.Message;
            if (!responseMessage.ContainsKey("success"))
            {
                return "Unrecognized response";
            }

            var success = (bool)responseMessage["success"];
            if (!success)
            {
                return responseMessage.ContainsKey("errorMessage") ? responseMessage["errorMessage"] as string : "Unknown reason";
            }

            return string.Empty;
        }
    }
}
