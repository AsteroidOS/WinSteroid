using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace WinSteroid.Common.Helpers
{
    public static class TilesHelper
    {
        const string BatteryTileId = "batteryTile";

        public static IAsyncOperation<bool> PinBatteryTileAsync(string deviceId, string devicename)
        {
            if (SecondaryTile.Exists(BatteryTileId)) return Task.FromResult(true).AsAsyncOperation();

            var batteryTile = new SecondaryTile(
                tileId: BatteryTileId,
                displayName: devicename + "'s battery charge",
                arguments: "deviceId=" + System.Net.WebUtility.UrlEncode(deviceId),
                square150x150Logo: new Uri("ms-appx:///Assets/Square150x150Logo.png"),
                desiredSize: TileSize.Default);

            return batteryTile.RequestCreateAsync();
        }

        public static IAsyncOperation<bool> UnpinBatteryTileAsync()
        {
            if (!SecondaryTile.Exists(BatteryTileId)) return Task.FromResult(true).AsAsyncOperation();

            var batteryTile = new SecondaryTile(BatteryTileId);
            if (batteryTile == null) return Task.FromResult(true).AsAsyncOperation();

            return batteryTile.RequestDeleteAsync();
        }

        public static bool BatteryTileExists() => SecondaryTile.Exists(BatteryTileId);

        public static void ResetBatteryTile() => UpdateBatteryTile(null);

        public static void UpdateBatteryTile(int? percentage)
        {
            if (!SecondaryTile.Exists(BatteryTileId)) return;

            var deviceId = SettingsHelper.GetValue("lastSavedDeviceId", string.Empty);
            var deviceName = SettingsHelper.GetValue("lastSavedDeviceName", string.Empty);

            var tileVisual = new TileVisual
            {
                DisplayName = nameof(WinSteroid),
                TileMedium = new TileBinding
                {
                    Content = new TileBindingContentAdaptive
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = percentage + "%",
                                HintStyle = AdaptiveTextStyle.Header,
                                HintAlign = AdaptiveTextAlign.Center
                            },
                            new AdaptiveText
                            {
                                Text = deviceName,
                                HintStyle = AdaptiveTextStyle.Base,
                                HintAlign = AdaptiveTextAlign.Left
                            }
                        }
                    }
                }
            };

            var tileContent = new TileContent
            {
                Visual = tileVisual
            };

            var tileNotification = new TileNotification(tileContent.GetXml());

            TileUpdateManager.CreateTileUpdaterForSecondaryTile(BatteryTileId).Update(tileNotification);
        }
    }
}
