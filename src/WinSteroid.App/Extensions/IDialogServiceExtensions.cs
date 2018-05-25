using System;
using System.Threading.Tasks;

namespace GalaSoft.MvvmLight.Views
{
    public static class IDialogServiceExtensions
    {
        public static Task ShowError(this IDialogService dialogService, Exception exception, string title)
        {
            return dialogService.ShowError(exception, title, null, () => { });
        }
    }
}
