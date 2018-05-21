using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using WinSteroid.App.ViewModels;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Controls
{
    public sealed partial class CustomIconDialog : ContentDialog
    {
        public readonly List<IconViewModel> Icons;

        public IconViewModel SelectedIcon { get; set; }

        public CustomIconDialog()
        {
            this.Icons = ApplicationIconExtensions.GetList()
                .Select(applicationIcon => new IconViewModel
                {
                    Id = applicationIcon.GetRealValue(),
                    Name = applicationIcon.ToString(),
                    Icon = applicationIcon
                })
                .ToList();

            this.InitializeComponent();
        }
    }
}
