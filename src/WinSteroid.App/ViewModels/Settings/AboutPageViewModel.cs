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

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;

namespace WinSteroid.App.ViewModels.Settings
{
    public class AboutPageViewModel : BasePageViewModel
    {
        public AboutPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.Initialize();
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override async void Initialize()
        {
            this.ApplicationName = Package.Current.DisplayName;
            this.ApplicationVersion = Package.Current.Id.GetVersion();
            if (!this.UsedSoftwares.IsNullOrEmpty()) return;

            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///THIRD-SOFTWARE.md"));
            var lines = file != null ? await FileIO.ReadLinesAsync(file) : new List<string>();
            this.UsedSoftwares = this.LoadSoftwareItems(lines);
        }

        public override void Reset()
        {

        }

        private string _applicationVersion;
        public string ApplicationVersion
        {
            get { return _applicationVersion; }
            set { Set(nameof(ApplicationVersion), ref _applicationVersion, value); }
        }

        private string _applicationName;
        public string ApplicationName
        {
            get { return _applicationName; }
            set { Set(nameof(ApplicationName), ref _applicationName, value); }
        }

        private List<SoftwareItem> _usedSoftwares;
        public List<SoftwareItem> UsedSoftwares
        {
            get { return _usedSoftwares; }
            set { Set(nameof(UsedSoftwares), ref _usedSoftwares, value); }
        }

        private RelayCommand _reviewCommand;
        public RelayCommand ReviewCommand
        {
            get
            {
                if (_reviewCommand == null)
                {
                    _reviewCommand = new RelayCommand(Review);
                }

                return _reviewCommand;
            }
        }

        private async void Review()
        {
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Package.Current.Id.FamilyName)));
        }

        private List<SoftwareItem> LoadSoftwareItems(IList<string> lines)
        {
            if (lines.IsNullOrEmpty()) return new List<SoftwareItem>();

            lines = lines
                .Skip(3)
                .Select(l => l?.Trim() ?? string.Empty)
                .ToList();

            if (lines.IsNullOrEmpty()) return new List<SoftwareItem>();

            var softwareItemsList = new List<SoftwareItem>();

            var currentItem = new SoftwareItem();

            foreach (var line in lines)
            {
                switch (line)
                {
                    case var l when !string.IsNullOrWhiteSpace(l) && l.Length >= 2 && l.Substring(0, 2).Equals("##"):
                        {
                            if (line.StartsWith("####")) //END
                            {
                                softwareItemsList.Add(currentItem);
                                currentItem = new SoftwareItem();
                            }
                            else
                            {
                                var softwareData = line.Replace("##", string.Empty).Split('(');
                                currentItem.SoftwareName = softwareData[0].Replace("[", string.Empty).Replace("]", string.Empty).Trim();
                                currentItem.SoftwareUrl = softwareData[1].Replace(")", string.Empty).Trim();
                            }
                            break;
                        }
                    default:
                        {
                            if (currentItem.License == null)
                            {
                                currentItem.License = string.Empty;
                                if (string.IsNullOrWhiteSpace(line)) break;
                            }

                            currentItem.License += line;
                            currentItem.License += Environment.NewLine;
                            break;
                        }
                }
            }

            return softwareItemsList;
        }
    }

    public class SoftwareItem
    {
        public string License { get; set; }

        public string SoftwareName { get; set; }

        public string SoftwareUrl { get; set; }
    }
}
