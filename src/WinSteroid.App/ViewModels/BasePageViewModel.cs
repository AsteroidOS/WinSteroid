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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Threading.Tasks;

namespace WinSteroid.App.ViewModels
{
    public abstract class BasePageViewModel : ViewModelBase
    {
        public readonly IDialogService DialogService;
        public readonly INavigationService NavigationService;

        public BasePageViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.NavigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        public abstract Task<bool> CanGoBack();

        public abstract void Initialize();

        public abstract void Reset();

        private string _busyMessage;
        public string BusyMessage
        {
            get { return _busyMessage; }
            set { Set(nameof(BusyMessage), ref _busyMessage, value); }
        }

        public bool Initialized { get; internal set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(nameof(IsBusy), ref _isBusy, value); }
        }

        private RelayCommand _backCommand;
        public RelayCommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                {
                    _backCommand = new RelayCommand(GoBack);
                }

                return _backCommand;
            }
        }

        private async void GoBack()
        {
            var canGoBack = await CanGoBack();
            if (!canGoBack) return;

            this.NavigationService.GoBack();
        }
    }
}
