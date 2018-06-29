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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace WinSteroid.App.ViewModels
{
    public abstract class BaseViewModel : ViewModelBase
    {
        public BaseViewModel()
        {
            this.MessengerInstance = Messenger.Default;
        }
        
        public abstract void Initialize();

        public abstract void Reset();

        private string _busyMessage;
        public string BusyMessage
        {
            get { return _busyMessage; }
            set { Set(nameof(BusyMessage), ref _busyMessage, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(nameof(IsBusy), ref _isBusy, value); }
        }
    }
}