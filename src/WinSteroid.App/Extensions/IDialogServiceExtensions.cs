﻿//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
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
using WinSteroid.Common.Helpers;

namespace GalaSoft.MvvmLight.Views
{
    public static class IDialogServiceExtensions
    {
        public static Task ShowError(this IDialogService dialogService, Exception exception, string title)
        {
            return dialogService.ShowError(exception, title, ResourcesHelper.GetLocalizedString("SharedOkMessage"), () => { });
        }

        public static Task ShowError(this IDialogService dialogService, string message, string title)
        {
            return dialogService.ShowError(message, title, ResourcesHelper.GetLocalizedString("SharedOkMessage"), () => { });
        }

        public static Task<bool> ShowConfirmMessage(this IDialogService dialogService, string message, string title)
        {
            return dialogService.ShowMessage(
                message, 
                title,
                buttonConfirmText: ResourcesHelper.GetLocalizedString("SharedYesMessage"),
                buttonCancelText: ResourcesHelper.GetLocalizedString("SharedNoMessage"), 
                afterHideCallback: b => { });
        }
    }
}
