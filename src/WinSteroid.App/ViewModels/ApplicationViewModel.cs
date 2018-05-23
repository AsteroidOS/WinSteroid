using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels
{
    public class ApplicationViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ApplicationIcon Icon { get; set; }

        public bool Muted { get; set; }

        public bool HasVibration { get; set; }
    }
}
