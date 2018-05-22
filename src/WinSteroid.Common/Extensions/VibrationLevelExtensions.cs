namespace WinSteroid.Common.Models
{
    public static class VibrationLevelExtensions
    {
        public static string GetId(this VibrationLevel vibrationLevel)
        {
            switch (vibrationLevel)
            {
                case VibrationLevel.None:
                default:
                    return "none";
                case VibrationLevel.Normal:
                    return "normal";
                case VibrationLevel.Strong:
                    return "strong";
            }
        }
    }
}
