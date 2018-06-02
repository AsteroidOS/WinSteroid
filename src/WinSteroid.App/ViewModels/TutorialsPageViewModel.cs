using System.Collections.Generic;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;

namespace WinSteroid.App.ViewModels
{
    public class TutorialsPageViewModel : BasePageViewModel
    {
        public TutorialsPageViewModel(IDialogService dialogService, INavigationService navigationService) : base(dialogService, navigationService)
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
                    PageKey = nameof(ViewModelLocator.TutorialUsb),
                    Glyph = ""
                }
            };
        }

        public override void Reset()
        {

        }

        private TutorialItem _selectedTutorialItem;
        public TutorialItem SelectedTutorialItem
        {
            get { return _selectedTutorialItem; }
            set
            {
                if (!Set(nameof(SelectedTutorialItem), ref _selectedTutorialItem, value)) return;

                if (_selectedTutorialItem == null) return;

                this.NavigationService.NavigateTo(_selectedTutorialItem.PageKey);
            }
        }

        private List<TutorialItem> _tutorialItems;
        public List<TutorialItem> TutorialItems
        {
            get { return _tutorialItems; }
            set { Set(nameof(TutorialItems), ref _tutorialItems, value); }
        }
    }

    public class TutorialItem
    {
        public string Title { get; set; }

        public string PageKey { get; set; }

        public string Glyph { get; set; }
    }
}
