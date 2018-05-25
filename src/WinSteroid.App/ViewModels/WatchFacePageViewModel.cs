using System;
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using WinSteroid.App.Controls;
using WinSteroid.App.Services;

namespace WinSteroid.App.ViewModels
{
    public class WatchFacePageViewModel : BasePageViewModel
    {
        private readonly ScpService ScpService;

        public WatchFacePageViewModel(
            ScpService scpService,
            IDialogService dialogService, 
            INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.ScpService = scpService ?? throw new ArgumentNullException(nameof(scpService));
        }

        public override void Initialize()
        {
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(!this.IsBusy);
        }

        private long _uploadProgress;
        public long UploadProgress
        {
            get { return _uploadProgress; }
            set { Set(nameof(UploadProgress), ref _uploadProgress, value); }
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
            var scpCredentialsDialog = new ScpCredentialsDialog();
            var result = await scpCredentialsDialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;

            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/WatchFaces/100-win-digital.qml"));

            var scpClient = this.ScpService.Connect(scpCredentialsDialog.HostIP, scpCredentialsDialog.Username, scpCredentialsDialog.Password);
            scpClient.Uploading += OnClientUploading;

            try
            {
                using (var randomAccessStream = await storageFile.OpenReadAsync())
                {
                    using (var stream = randomAccessStream.AsStream())
                    {
                        scpClient.Upload(stream, "/usr/share/asteroid-launcher/watchfaces/" + storageFile.Name);
                    }
                }
            }
            catch (Exception exception)
            {
                await this.DialogService.ShowError(exception, "SCP Connection Error", null, () => { });
            }

            scpClient.Uploading -= OnClientUploading;
            scpClient.Disconnect();
            scpClient.Dispose();
        }

        private void OnClientUploading(object sender, Renci.SshNet.Common.ScpUploadEventArgs args)
        {
            this.UploadProgress = ((args.Size - args.Uploaded) * 100) / args.Size;
        }
    }
}
