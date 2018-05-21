using System.ComponentModel.DataAnnotations;

namespace WinSteroid.Common.Models
{
    public enum ApplicationIcon
    {
        [Display(Name = "ios-alert")]
        Alert,
        [Display(Name = "ios-apps")]
        Apps,
        [Display(Name = "ios-calendar")]
        Calendar,
        [Display(Name = "ios-call")]
        Call,
        [Display(Name = "ios-mail")]
        Message,
        [Display(Name = "ios-camera")]
        Photo,
        [Display(Name = "ios-settings")]
        Settings,
        [Display(Name = "ios-send")]
        SMS
    }
}
