using WinSteroid.Common.Models;

namespace WinSteroid.App.ViewModels
{
    public class ApplicationPreferenceViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public ApplicationIcon Icon { get; set; }

        public bool Muted { get; set; }
    }
}
