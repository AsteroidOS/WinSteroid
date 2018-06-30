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
using GalaSoft.MvvmLight.Threading;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using WinSteroid.Common;
using WinSteroid.Common.Bluetooth;
using WinSteroid.Common.Helpers;
using WinSteroid.Common.Messages;

namespace WinSteroid.App.ViewModels.Controls
{
    public class ScreenshotsBenchmarkDialogViewModel : BaseViewModel
    {
        public override void Initialize()
        {
            this.TestGlyph = "";
            this.ErrorMessage = string.Empty;
            this.TestBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;

            this.MessengerInstance.Register<ScreenshotBenchmarkMessage>(this, LoadBenchmarkResults);
        }

        public override void Reset()
        {
            
        }

        private string _testGlyph;
        public string TestGlyph
        {
            get { return _testGlyph; }
            set { Set(nameof(TestGlyph), ref _testGlyph, value); }
        }

        private bool _testPassed;
        public bool TestPassed
        {
            get { return _testPassed; }
            set { Set(nameof(TestPassed), ref _testPassed, value); }
        }

        private SolidColorBrush _testBrush;
        public SolidColorBrush TestBrush
        {
            get { return _testBrush; }
            set { Set(nameof(TestBrush), ref _testBrush, value); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { Set(nameof(ErrorMessage), ref _errorMessage, value); }
        }

        private RelayCommand _startBenchmarkCommand;
        public RelayCommand StartBenchmarkCommand
        {
            get
            {
                if (_startBenchmarkCommand == null)
                {
                    _startBenchmarkCommand = new RelayCommand(StartBenchmark);
                }

                return _startBenchmarkCommand;
            }
        }

        private async void StartBenchmark()
        {
            var screenshotServiceReady = await DeviceManager.RegisterToScreenshotContentServiceBenchmark();
            if (!screenshotServiceReady)
            {
                this.ErrorMessage = ResourcesHelper.GetLocalizedString("ScreenshotServiceInitializationErrorMessage");
                return;
            }

            this.IsBusy = true;
            await DeviceManager.TestScreenshotContentServiceAsync();
        }

        private async void LoadBenchmarkResults(ScreenshotBenchmarkMessage message)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                this.IsBusy = false;
                this.TestGlyph = message.ServiceReady ? "" : "";
                this.TestPassed = message.ServiceReady;
                this.TestBrush = new SolidColorBrush(message.ServiceReady ? Colors.Green : Colors.Red);
                this.ErrorMessage = message.ServiceReady
                    ? string.Empty
                    : string.Format(ResourcesHelper.GetLocalizedString("ScreenshotServiceMaxDtuSizeErrorMessageFormat"), Asteroid.CurrentMinimalBtsyncdPacketSize);
            });
        }
    }
}
