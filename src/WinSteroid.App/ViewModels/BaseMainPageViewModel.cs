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

using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;

namespace WinSteroid.App.ViewModels
{
    public abstract class BaseMainPageViewModel : BasePageViewModel
    {
        public BaseMainPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.MenuOptions = new ObservableCollection<MenuOptionViewModel>();
            this.InitializeMenuOptions();
        }

        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set { Set(nameof(IsMenuOpen), ref _isMenuOpen, value); }
        }

        public abstract void InitializeMenuOptions();

        private ObservableCollection<MenuOptionViewModel> _menuOptions;
        public ObservableCollection<MenuOptionViewModel> MenuOptions
        {
            get { return _menuOptions; }
            set { Set(nameof(MenuOptions), ref _menuOptions, value); }
        }

        public void ManageSelectedMenuOption(MenuOptionViewModel menuOption)
        {
            this.IsMenuOpen = false;

            if (menuOption == null || menuOption.Command == null || !menuOption.Command.CanExecute(null)) return;

            menuOption.Command.Execute(null);
        }

        private RelayCommand _menuCommand;
        public RelayCommand MenuCommand
        {
            get
            {
                if (_menuCommand == null)
                {
                    _menuCommand = new RelayCommand(ToggleMenu);
                }

                return _menuCommand;
            }
        }

        private void ToggleMenu()
        {
            this.IsMenuOpen = !this.IsMenuOpen;
        }
    }
}
