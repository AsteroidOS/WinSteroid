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
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using WinSteroid.App.Controls;
using WinSteroid.App.Factory;
using WinSteroid.Common.Helpers;

namespace WinSteroid.App.ViewModels
{
    public class WatchFacePageViewModel : BasePageViewModel
    {
        public WatchFacePageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {

        }

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

        private int _uploadProgress;
        public int UploadProgress
        {
            get { return _uploadProgress; }
            set { Set(nameof(UploadProgress), ref _uploadProgress, value); }
        }

        private bool _isUploading;
        public bool IsUploading
        {
            get { return _isUploading; }
            set { Set(nameof(IsUploading), ref _isUploading, value); }
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

            var file = await FilesHelper.PickFileAsync(".qml");
            if (file == null) return;

            this.SelectedFile = file;
            this.SelectedFileName = file?.Name;
            this.IsBusy = false;
        }

        private RelayCommand _connectCommand;
        public RelayCommand ConnectCommand
        {
            get
            {
                if (_connectCommand == null)
                {
                    _connectCommand = new RelayCommand(Connect);
                }

                return _connectCommand;
            }
        }

        private async void Connect()
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
                using (var scpClient = SecureConnectionsFactory.CreateScpClient(scpCredentialsDialog.HostIP, scpCredentialsDialog.Username, scpCredentialsDialog.Password))
                {
                    scpClient.ErrorOccurred += OnClientErrorOccured;
                    scpClient.Uploading += OnClientUploading;

                    using (var randomAccessStream = await this.SelectedFile.OpenReadAsync())
                    {
                        using (var stream = randomAccessStream.AsStream())
                        {
                            scpClient.Upload(stream, "/usr/share/asteroid-launcher/watchfaces/" + this.SelectedFile.Name);
                        }
                    }

                    scpClient.Uploading -= OnClientUploading;
                    scpClient.ErrorOccurred -= OnClientErrorOccured;
                }

                successfulUpload = true;
            }
            catch (Exception exception)
            {
                await this.DialogService.ShowError(exception, "SCP Connection Error");
            }

            this.IsUploading = false;
            this.UploadProgress = 0;

            if (successfulUpload)
            {
                ToastsHelper.Show("Watchface successfully installed");
            }

            //if (!successfulUpload)
            //{
            //    this.IsBusy = false;
            //    return;
            //}

            //var restartSystem = await this.DialogService.ShowMessage(
            //    message: "AsteroidOS may cache the current watchface. Do you want to restart device to refresh it?",
            //    title: "WatchFace uploaded",
            //    buttonConfirmText: "Yes",
            //    buttonCancelText: "No",
            //    afterHideCallback: b => { });

            //if (!restartSystem)
            //{
            //    this.IsBusy = false;
            //    return;
            //}

            //try
            //{
            //    using (var sshClient = SecureConnectionsHelper.CreateSshClient(scpCredentialsDialog.HostIP, scpCredentialsDialog.Username, scpCredentialsDialog.Password))
            //    {
            //        sshClient.ErrorOccurred += OnClientErrorOccured;

            //        sshClient.RunCommand("systemctl restart user@1000");

            //        sshClient.ErrorOccurred -= OnClientErrorOccured;
            //    }
            //}
            //catch (Exception exception)
            //{
            //    await this.DialogService.ShowError(exception, "SSH Connection Error");
            //}

            this.IsBusy = false;
        }

        private void OnClientUploading(object sender, Renci.SshNet.Common.ScpUploadEventArgs args)
        {
            this.UploadProgress = Convert.ToInt32(((args.Size - args.Uploaded) * 100) / args.Size);
        }

        private async void OnClientErrorOccured(object sender, Renci.SshNet.Common.ExceptionEventArgs args)
        {
            await this.DialogService.ShowError(args.Exception, "Client Error");
        }
    }
}
