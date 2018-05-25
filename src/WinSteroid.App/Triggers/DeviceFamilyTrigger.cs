using Windows.UI.Xaml;
using WinSteroid.Common.Models;

namespace WinSteroid.App.Triggers
{
    public class DeviceFamilyTrigger : StateTriggerBase
    {
        public DeviceFamily DeviceFamily
        {
            get { return (DeviceFamily)GetValue(DeviceFamilyProperty); }
            set { SetValue(DeviceFamilyProperty, value); }
        }
        
        public static readonly DependencyProperty DeviceFamilyProperty =
            DependencyProperty.Register(nameof(DeviceFamily), typeof(DeviceFamily), typeof(DeviceFamilyTrigger), new PropertyMetadata(default(DeviceFamily), OnDeviceFamilyPropertyChanged));

        private static void OnDeviceFamilyPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var trigger = (DeviceFamilyTrigger)dependencyObject;
            var newDeviceFamily = (DeviceFamily)args.NewValue;
            trigger.SetActive(newDeviceFamily == DeviceFamilyExtensions.CurrentDeviceFamily);
        }
    }
}
