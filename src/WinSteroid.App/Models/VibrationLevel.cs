using System.ComponentModel.DataAnnotations;

namespace WinSteroid.App.Models
{
    public enum VibrationLevel
    {
        [Display(Name = "none")]
        None = 0,
        [Display(Name = "normal")]
        Normal = 1,
        [Display(Name = "strong")]
        Strong = 2
    }
}
