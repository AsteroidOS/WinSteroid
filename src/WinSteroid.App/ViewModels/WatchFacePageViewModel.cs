using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
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

            this.ScpService.Connect(scpCredentialsDialog.HostIP, scpCredentialsDialog.Username, scpCredentialsDialog.Password);
            this.ScpService.AttachUploadProgressHandler(OnClientUploading);
        }

        private void OnClientUploading(object sender, Renci.SshNet.Common.ScpUploadEventArgs args)
        {
            this.UploadProgress = ((args.Size - args.Uploaded) * 100) / args.Size;
        }
    }
}
