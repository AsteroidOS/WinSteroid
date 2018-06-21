using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;
using Windows.ApplicationModel;
using Windows.Storage;

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
                                currentItem.License += Environment.NewLine;
                                softwareItemsList.Add(currentItem);
                                currentItem = new SoftwareItem();
                            }
                            else if (line.StartsWith("###")) //AUTHOR
                            {
                                var authorsData = line.Replace("###", string.Empty).Split('(');
                                currentItem.Author = authorsData[0].Trim();
                                currentItem.AuthorUrl = authorsData[1].Replace(")", string.Empty).Trim();
                            }
                            else
                            {
                                var softwareData = line.Replace("##", string.Empty).Split('(');
                                currentItem.SoftwareName = softwareData[0].Trim();
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
                            break;
                        }
                }
            }

            return softwareItemsList;
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
    }

    public class SoftwareItem
    {
        public string SoftwareName { get; set; }

        public string Author { get; set; }

        public string AuthorUrl { get; set; }

        public string License { get; set; }

        public string SoftwareUrl { get; set; }
    }
}
