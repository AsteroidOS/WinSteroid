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

using GalaSoft.MvvmLight.Messaging;
using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WinSteroid.App.ViewModels;
using WinSteroid.App.ViewModels.Transfers;

namespace WinSteroid.App.Views.Transfers
{
    public sealed partial class WallpapersPage : Page
    {
        public WallpapersPageViewModel ViewModel
        {
            get { return this.DataContext as WallpapersPageViewModel; }
        }

        public WallpapersPage()
        {
            this.InitializeComponent();
            Messenger.Default.Register<StorageFile>(this, nameof(ViewModelLocator.TransfersWallpapers), LoadImage);
        }
        
        public async void LoadImage(StorageFile storageFile)
        {
            this.ViewModel.IsImageBusy = true;

            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.ImageFailed += OnImageFailed;
                bitmapImage.ImageOpened += OnImageOpened;

                this.WallpaperPreviewImage.Source = bitmapImage;

                var stream = await storageFile.OpenStreamForReadAsync();

                await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
            }
            catch
            {
                this.LoadDefaultImage();
            }
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            this.ViewModel.IsImageBusy = false;
        }

        private async void LoadDefaultImage()
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/default.png"));

            this.LoadImage(storageFile);
        }

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            this.ViewModel.IsImageBusy = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.ViewModel.SelectedFile != null)
            {
                this.LoadImage(this.ViewModel.SelectedFile);
            }

            base.OnNavigatedTo(e);
        }
    }
}
