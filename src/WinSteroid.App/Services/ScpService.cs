using GalaSoft.MvvmLight.Views;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace WinSteroid.App.Services
{
    public class ScpService
    {
        private readonly IDialogService DialogService;

        public ScpService(IDialogService dialogService)
        {
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        private ScpClient Client { get; set; }

        public bool Connect(string ip, string username, string password)
        {
            if (this.Client == null)
            {
                this.Client = new ScpClient(ip, username, password);
                this.Client.ErrorOccurred += OnErrorOccured;
            }

            if (this.Client.IsConnected) return true;

            try
            {
                this.Client.Connect();
                return this.Client.IsConnected;
            }
            catch (Exception exception)
            {
                this.DialogService.ShowError(exception, "SCP Connection Error", null, () => { });
                return false;
            }
        }

        private async void OnErrorOccured(object sender, ExceptionEventArgs args)
        {
            await this.DialogService.ShowError(args.Exception, "SCP Upload Error", null, () => { });
        }

        public void Disconnect()
        {
            if (this.Client == null || !this.Client.IsConnected) return;

            this.Client.Disconnect();
            this.Client.Dispose();
            this.Client = null;
        }

        public void AttachUploadProgressHandler(EventHandler<ScpUploadEventArgs> progressHandler)
        {
            this.Client.Uploading += progressHandler;
        }

        public async Task<bool> UploadAsync(IStorageFile storageFile)
        {
            if (!this.Client.IsConnected) return false;

            try
            {
                using (var randomAccessStream = await storageFile.OpenReadAsync())
                {
                    using (var stream = randomAccessStream.AsStream())
                    {
                        this.Client.Upload(randomAccessStream.AsStream(), "/");
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
