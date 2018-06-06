using GalaSoft.MvvmLight.Command;

namespace WinSteroid.App.ViewModels
{
    public class MenuOptionViewModel
    {
        public string Glyph { get; set; }

        public string Label { get; set; }

        public RelayCommand Command { get; set; }
    }
}
