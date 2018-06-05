using System.Collections.Generic;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;

namespace WinSteroid.App.ViewModels.Tutorials
{
    public class MainPageViewModel : BasePageViewModel
    {
        public MainPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
        {
            this.Initialize();
        }

        public override Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {
            this.TutorialItems = new List<TutorialItem>
            {
                new TutorialItem
                {
                    Title = "How to connect an AsteroidOS watch to PC",
                    PageKey = nameof(ViewModelLocator.TutorialsUsb),
                    Glyph = ""
                }
            };
        }

        public override void Reset()
        {

        }

        private List<TutorialItem> _tutorialItems;
        public List<TutorialItem> TutorialItems
        {
            get { return _tutorialItems; }
            set { Set(nameof(TutorialItems), ref _tutorialItems, value); }
        }

        public void NavigateTo(string pageKey)
        {
            this.NavigationService.NavigateTo(pageKey);
        }
    }

    public class TutorialItem
    {
        public string Title { get; set; }

        public string PageKey { get; set; }

        public string Glyph { get; set; }
    }
}
