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
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using WinSteroid.App.Controls;
using WinSteroid.Common.Factory;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels.Transfers
{
    public class WallpapersPageViewModel : BasePageViewModel
    {
        public WallpapersPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
        }

        private const int DefaultImageSize = 480;

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(!this.IsBusy);
        }

        public override void Initialize()
        {

        }

        public override void Reset()
        {

        }

        private bool _isImageBusy;
        public bool IsImageBusy
        {
            get { return _isImageBusy; }
            set { Set(nameof(IsImageBusy), ref _isImageBusy, value); }
        }

        private bool _isUploading;
        public bool IsUploading
        {
            get { return _isUploading; }
            set { Set(nameof(IsUploading), ref _isUploading, value); }
        }

        private bool _useRoundPreview;
        public bool UseRoundPreview
        {
            get { return _useRoundPreview; }
            set { Set(nameof(UseRoundPreview), ref _useRoundPreview, value); }
        }

        private StorageFile _selectedFile;
        public StorageFile SelectedFile
        {
            get { return _selectedFile; }
            set { Set(nameof(SelectedFile), ref _selectedFile, value); }
        }

        private string _selectedFileName;
        public string SelectedFileName
        {
            get { return _selectedFileName; }
            private set { Set(nameof(SelectedFileName), ref _selectedFileName, value); }
        }

        private RelayCommand _loadFileCommand;
        public RelayCommand LoadFileCommand
        {
            get
            {
                if (_loadFileCommand == null)
                {
                    _loadFileCommand = new RelayCommand(LoadFile);
                }

                return _loadFileCommand;
            }
        }

        private async void LoadFile()
        {
            this.IsBusy = true;

            var file = await FilesHelper.PickFileAsync(".jpeg", ".jpg");
            if (file == null)
            {
                this.IsBusy = false;
                return;
            }

            ImageProperties imageProperties = null;

            try
            {
                imageProperties = await file.Properties.GetImagePropertiesAsync();
            }
            catch (Exception exception)
            {
                //Corrupted image? Or file isn't an image after all...
                var message = App.InDebugMode ? exception.ToString() : ResourcesHelper.GetLocalizedString("SettingsWallpapersInvalidImageMessage");

                await this.DialogService.ShowError(message, ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
            }

            if (imageProperties == null) return;

            if (imageProperties.Width != imageProperties.Height)
            {
                await this.DialogService.ShowError(
                    ResourcesHelper.GetLocalizedString("SettingsWallpapersUnsupportedImageErrorMessage"),
                    ResourcesHelper.GetLocalizedString("SettingsWallpapersUnsupportedImageTitle"));

                this.IsBusy = false;
                return;

                //var cropImageTaskAllowed = await this.DialogService.ShowConfirmMessage(
                //    ResourcesHelper.GetLocalizedString("SettingsWallpapersUnsupportedImageMessage"),
                //    ResourcesHelper.GetLocalizedString("SettingsWallpapersUnsupportedImageTitle"));
                //if (!cropImageTaskAllowed)
                //{
                //    this.IsBusy = false;
                //    return;
                //}

                //file = await ImageHelper.CropImageFileAsync(file, cropWidthPixels: DefaultImageSize, cropHeightPixels: DefaultImageSize);
            }

            this.SelectedFile = file;
            this.SelectedFileName = file?.Name;
            this.IsBusy = false;

            this.MessengerInstance.Send(file, nameof(ViewModelLocator.TransfersWallpapers));
        }

        private RelayCommand _uploadCommand;
        public RelayCommand UploadCommand
        {
            get
            {
                if (_uploadCommand == null)
                {
                    _uploadCommand = new RelayCommand(Upload);
                }

                return _uploadCommand;
            }
        }

        private async void Upload()
        {
            if (this.SelectedFile == null) return;

            var successfulUpload = false;
            this.IsBusy = true;

            var scpCredentialsDialog = new ScpCredentialsDialog();
            var result = await scpCredentialsDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                this.IsBusy = false;
                return;
            }

            this.IsUploading = true;

            try
            {
                using (var scpClient = SecureConnectionsFactory.CreateScpClient(
                    scpCredentialsDialog.ViewModel.HostIP, 
                    scpCredentialsDialog.ViewModel.Username, 
                    scpCredentialsDialog.ViewModel.Password))
                {
                    scpClient.ErrorOccurred += OnClientErrorOccured;

                    var bytes = await ImageHelper.EncodeToSquareJpegImageAsync(this.SelectedFile, DefaultImageSize);

                    using (var memoryStream = new MemoryStream())
                    {
                        memoryStream.Write(bytes, 0, bytes.Length);
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        scpClient.Upload(memoryStream, "/usr/share/asteroid-launcher/wallpapers/WinSteroid_" + this.SelectedFile.Name);
                    }
                    
                    scpClient.ErrorOccurred -= OnClientErrorOccured;
                }

                successfulUpload = true;
            }
            catch (Exception exception)
            {
                await this.DialogService.ShowError(exception, ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
            }

            this.IsUploading = false;

            if (successfulUpload)
            {
                ToastsHelper.Show(ResourcesHelper.GetLocalizedString("SettingsWallpapersWallpaperInstalledMessage"));
            }

            this.IsBusy = false;
        }

        private async void OnClientErrorOccured(object sender, Renci.SshNet.Common.ExceptionEventArgs args)
        {
            await this.DialogService.ShowError(args.Exception, ResourcesHelper.GetLocalizedString("SharedErrorTitle"));
        }
    }
}
