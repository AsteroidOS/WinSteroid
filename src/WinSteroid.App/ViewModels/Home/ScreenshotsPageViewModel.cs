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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using WinSteroid.App.Messages;
using WinSteroid.App.Services;
using WinSteroid.Common;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels.Home
{
    public class ScreenshotsPageViewModel : BasePageViewModel
    {
        private readonly DeviceService DeviceService;

        public ScreenshotsPageViewModel(
            IDialogService dialogService, 
            INavigationService navigationService,
            DeviceService deviceService) : base(dialogService, navigationService)
        {
            this.DeviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));

            this.Initialize();
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override async void Initialize()
        {
            this.ScreenshotName = string.Empty;

            this.MessengerInstance.Register<ScreenshotAcquiredMessage>(this, LoadScreenshotPreview);
            this.MessengerInstance.Register<ScreenshotBenchmarkMessage>(this, LoadBenchmarkResults);
            this.MessengerInstance.Register<ScreenshotProgressMessage>(this, UpdateScreenshotProgress);

            var defaultImageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/default.png"));

            this.ScreenshotPreview = new BitmapImage();

            using (var ras = await defaultImageFile.OpenReadAsync())
            {
                await this.ScreenshotPreview.SetSourceAsync(ras);
            }

            this.UpdateScreenshotHistoryFilesCount();
        }

        public override void Reset()
        {

        }
        
        private int _screenshotHistoryFilesCount;
        public int ScreenshotHistoryFilesCount
        {
            get { return _screenshotHistoryFilesCount; }
            set { Set(nameof(ScreenshotHistoryFilesCount), ref _screenshotHistoryFilesCount, value); }
        }

        private string _screenshotName;
        public string ScreenshotName
        {
            get { return _screenshotName; }
            set { Set(nameof(ScreenshotName), ref _screenshotName, value); }
        }

        private BitmapImage _screenshotPreview;
        public BitmapImage ScreenshotPreview
        {
            get { return _screenshotPreview; }
            set { Set(nameof(ScreenshotPreview), ref _screenshotPreview, value); }
        }

        private int _screenshotProgress;
        public int ScreenshotProgress
        {
            get { return _screenshotProgress; }
            set { Set(nameof(ScreenshotProgress), ref _screenshotProgress, value); }
        }

        private bool _screenshotSuccessfullyExported;
        public bool ScreenshotSuccessfullyExported
        {
            get { return _screenshotSuccessfullyExported; }
            set { Set(nameof(ScreenshotSuccessfullyExported), ref _screenshotSuccessfullyExported, value); }
        }

        private async void StartBenchmark()
        {
            var screenshotServiceReady = await this.DeviceService.RegisterToScreenshotContentServiceBenchmark();
            if (!screenshotServiceReady)
            {
                await this.DialogService.ShowError(
                    ResourcesHelper.GetLocalizedString("ScreenshotServiceInitializationErrorMessage"),
                    ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
                return;
            }

            await this.DeviceService.TestScreenshotContentServiceAsync();
        }

        private RelayCommand _takeScreenshotCommand;
        public RelayCommand TakeScreenshotCommand
        {
            get
            {
                if (_takeScreenshotCommand == null)
                {
                    _takeScreenshotCommand = new RelayCommand(this.TakeScreenshot);
                }

                return _takeScreenshotCommand;
            }
        }

        private async void TakeScreenshot()
        {
            var screenshotServiceReady = await this.DeviceService.RegisterToScreenshotContentService();
            if (!screenshotServiceReady)
            {
                await this.DialogService.ShowError(
                    ResourcesHelper.GetLocalizedString("ScreenshotServiceInitializationErrorMessage"),
                    ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
                return;
            }

            this.IsBusy = true;

            await this.DeviceService.TakeScreenshotAsync();
        }

        private RelayCommand _cancelScreenshotTaskCommand;
        public RelayCommand CancelScreenshotTaskCommand
        {
            get
            {
                if (_cancelScreenshotTaskCommand == null)
                {
                    _cancelScreenshotTaskCommand = new RelayCommand(CancelScreenshotTask, this.IsBusy);
                }

                return _cancelScreenshotTaskCommand;
            }
        }

        private async void CancelScreenshotTask()
        {
            await this.DeviceService.UnregisterToScreenshotContentService();

            this.ScreenshotProgress = 0;
            this.IsBusy = false;
        }

        private RelayCommand _exportCommand;
        public RelayCommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new RelayCommand(ExportToPictureLibrary);
                }

                return _exportCommand;
            }
        }

        private async void ExportToPictureLibrary()
        {
            var screenshotsFolder = await FilesHelper.GetScreenshotsFolderAsync();

            var file = await screenshotsFolder.GetFileAsync(this.ScreenshotName);
            if (file == null)
            {
                await this.DialogService.ShowError(
                    string.Format(ResourcesHelper.GetLocalizedString("SharedFileNotFoundMessageFormat"), this.ScreenshotName),
                    ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
                return;
            }

            var folder = await KnownFolders.PicturesLibrary.CreateFolderAsync(Package.Current.DisplayName, CreationCollisionOption.OpenIfExists);

            await file.CopyOrReplaceAsync(folder);

            this.ScreenshotSuccessfullyExported = true;
        }

        private RelayCommand _exportHistoryCommand;
        public RelayCommand ExportHistoryCommand
        {
            get
            {
                if (_exportHistoryCommand == null)
                {
                    _exportHistoryCommand = new RelayCommand(ExportScreenshotsHistory, this.ScreenshotHistoryFilesCount > 0);
                }

                return _exportHistoryCommand;
            }
        }

        private async void ExportScreenshotsHistory()
        {
            var screenshotsFolder = await FilesHelper.GetScreenshotsFolderAsync();

            var files = await screenshotsFolder.GetFilesAsync();
            if (files.IsNullOrEmpty()) return;

            var folder = await KnownFolders.PicturesLibrary.CreateFolderAsync(Package.Current.DisplayName);

            foreach (var file in files)
            {
                await file.CopyOrReplaceAsync(folder);
            }

            await this.DialogService.ShowMessage(
                ResourcesHelper.GetLocalizedString("HomeScreenshotsHistoryExportedMessage"), 
                ResourcesHelper.GetLocalizedString("SharedOperationCompletedTitle"));
        }

        private RelayCommand _deleteHistoryCommand;
        public RelayCommand DeleteHistoryCommand
        {
            get
            {
                if (_deleteHistoryCommand == null)
                {
                    _deleteHistoryCommand = new RelayCommand(DeleteScreenshotsHistory, this.ScreenshotHistoryFilesCount > 0);
                }

                return _deleteHistoryCommand;
            }
        }

        private async void DeleteScreenshotsHistory()
        {
            var confirmed = await this.DialogService.ShowConfirmMessage(
                ResourcesHelper.GetLocalizedString("HomeScreenshotsDeleteScreenshotsHistoryMessage"),
                ResourcesHelper.GetLocalizedString("HomeScreenshotsDeleteScreenshotsHistoryTitle"));

            if (!confirmed) return;

            var screenshotsFolder = await FilesHelper.GetScreenshotsFolderAsync();
            if (screenshotsFolder != null)
            {
                await screenshotsFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            this.UpdateScreenshotHistoryFilesCount();
        }

        private async void LoadBenchmarkResults(ScreenshotBenchmarkMessage message)
        {
            if (message.ServiceReady) return;

            await this.DialogService.ShowMessage(
                string.Format(ResourcesHelper.GetLocalizedString("ScreenshotServiceMaxDtuSizeErrorMessageFormat"), Asteroid.CurrentMinimalBtsyncdPacketSize),
                ResourcesHelper.GetLocalizedString("SharedUnavailableServiceTitle"));
        }

        private async void LoadScreenshotPreview(ScreenshotAcquiredMessage message)
        {
            await DispatcherHelper.RunAsync(async () =>
            {
                var screenshotsFolder = await FilesHelper.GetScreenshotsFolderAsync();

                var file = await screenshotsFolder.GetFileAsync(message.FileName);
                if (file == null)
                {
                    await this.DialogService.ShowError(
                        string.Format(ResourcesHelper.GetLocalizedString("SharedFileNotFoundMessageFormat"), message.FileName),
                        ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
                    return;
                }

                this.ScreenshotName = message.FileName;

                this.ScreenshotPreview = new BitmapImage();

                using (var ras = await file.OpenReadAsync())
                {
                    await this.ScreenshotPreview.SetSourceAsync(ras);
                }

                this.IsBusy = false;

                this.ScreenshotSuccessfullyExported = false;

                this.UpdateScreenshotHistoryFilesCount();
            }); 
        }

        private async void UpdateScreenshotProgress(ScreenshotProgressMessage message)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (this.ScreenshotProgress != message.Percentage)
                {
                    this.ScreenshotProgress = message.Percentage;
                }
            });
        }

        private async void UpdateScreenshotHistoryFilesCount()
        {
            var screenshotsFolder = await FilesHelper.GetScreenshotsFolderAsync();

            var files = await screenshotsFolder.GetFilesAsync();

            this.ScreenshotHistoryFilesCount = files.Count;
        }
    }
}
