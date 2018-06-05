using System.Collections.Generic;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;

namespace WinSteroid.App.ViewModels.Tutorials
{
    public class UsbPageViewModel : BasePageViewModel
    {
        public UsbPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {

        }

        public override void Reset()
        {

        }

        private List<Step> _steps;
        public List<Step> Steps
        {
            get
            {
                if (_steps == null)
                {
                    _steps = new List<Step>
                    {
                        new Step { Header = "Requirements", Title = "Requirements" },
                        new Step { Header = "Device Manager 1/2", Title = "Device Manager 1/2" },
                        new Step { Header = "Device Manager 2/2", Title = "Device Manager 2/2" }
                    };
                }

                return _steps;
            }
        }
    }

    public class Step
    {
        public string Header { get; set; }

        public string Title { get; set; }

        public string MarkdownText { get; set; }

        public string MediaSource { get; set; }
    }
}
