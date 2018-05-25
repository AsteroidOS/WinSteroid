﻿using GalaSoft.MvvmLight.Views;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;

namespace WinSteroid.App.Services
{
    public class ScpService
    {
        private readonly IDialogService DialogService;

        public ScpService(IDialogService dialogService)
        {
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }
        
        public ScpClient Connect(string ip, string username, string password)
        {
            var authenticationsMethods = new AuthenticationMethod[]
            {
                new NoneAuthenticationMethod(username)
            };

            var connectionInfo = new ConnectionInfo(ip, 22, username, authenticationsMethods);

            var client = new ScpClient(connectionInfo);
            client.ErrorOccurred += OnErrorOccured;

            if (client.IsConnected) return client;

            client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
            client.Connect();

            return client;
        }

        private async void OnErrorOccured(object sender, ExceptionEventArgs args)
        {
            await this.DialogService.ShowError(args.Exception, "SCP Upload Error", null, () => { });
        }
    }
}
