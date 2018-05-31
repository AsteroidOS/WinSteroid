//Copyright (C) 2018 - Luca Montanari <thunderluca93@gmail.com>
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.

using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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
                displayName: Package.Current.DisplayName,
                arguments: "deviceId=" + System.Net.WebUtility.UrlEncode(deviceId),
                square150x150Logo: new Uri("ms-appx:///Assets/Square150x150Logo.png"),
                desiredSize: TileSize.Default);

            batteryTile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
            batteryTile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Square310x310Logo.png");
            batteryTile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");

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
                Branding = TileBranding.NameAndLogo,
                TileMedium = CreateMediumTileBinding(percentage ?? 0, deviceName),
                TileWide = CreateWideTileBinding(percentage ?? 0, deviceName),
                TileLarge = CreateLargeTileBinding(percentage ?? 0, deviceName)
            };

            var tileContent = new TileContent
            {
                Visual = tileVisual
            };

            var tileNotification = new TileNotification(tileContent.GetXml());

            TileUpdateManager.CreateTileUpdaterForSecondaryTile(BatteryTileId).Update(tileNotification);
        }

        private static TileBinding CreateMediumTileBinding(int percentage, string deviceName)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    TextStacking = TileTextStacking.Center,
                    Children =
                        {
                            new AdaptiveText
                            {
                                Text = percentage > 99 ? percentage.ToString() : percentage + "%",
                                HintStyle = AdaptiveTextStyle.Header,
                                HintAlign = AdaptiveTextAlign.Center
                            },
                            new AdaptiveText
                            {
                                Text = deviceName,
                                HintStyle = AdaptiveTextStyle.SubtitleSubtle,
                                HintAlign = AdaptiveTextAlign.Center
                            }
                        }
                }
            };
        }

        private static TileBinding CreateWideTileBinding(int percentage, string deviceName)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    Children =
                    {
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = percentage + "%",
                                            HintStyle = AdaptiveTextStyle.Header,
                                            HintAlign = AdaptiveTextAlign.Left
                                        }
                                    }
                                },
                                new AdaptiveSubgroup
                                {
                                    HintTextStacking = AdaptiveSubgroupTextStacking.Top,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = deviceName,
                                            HintStyle = AdaptiveTextStyle.Title,
                                            HintAlign = AdaptiveTextAlign.Right
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static TileBinding CreateLargeTileBinding(int percentage, string deviceName)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    TextStacking = TileTextStacking.Center,
                    Children =
                    {
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 1
                                },
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 2,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = percentage + "%",
                                            HintStyle = AdaptiveTextStyle.Header,
                                            HintAlign = AdaptiveTextAlign.Center
                                        }
                                    }
                                },
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 1
                                }
                            }
                        },
                        new AdaptiveText
                        {
                            Text = deviceName,
                            HintStyle = AdaptiveTextStyle.SubtitleSubtle,
                            HintAlign = AdaptiveTextAlign.Center
                        }
                    }
                }
            };
        }
    }
}
