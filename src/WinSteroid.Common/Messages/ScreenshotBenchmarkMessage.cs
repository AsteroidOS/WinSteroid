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

namespace WinSteroid.Common.Messages
{
    public class ScreenshotBenchmarkMessage
    {
        public ScreenshotBenchmarkMessage(int averagePacketSize, long elapsedMilliseconds)
        {
            this.AveragePacketSize = averagePacketSize;
            this.ServiceReady = averagePacketSize >= Asteroid.CurrentMinimalBtsyncdPacketSize;
            this.ElapsedMilliseconds = elapsedMilliseconds;
            this.SpeedQuality = this.GetSpeedQualityByTime(elapsedMilliseconds);
        }

        public int AveragePacketSize { get; }
        public bool ServiceReady { get; }
        public long ElapsedMilliseconds { get; }
        public Speed SpeedQuality { get; }

        private Speed GetSpeedQualityByTime(long milliseconds)
        {
            switch (milliseconds)
            {
                case var m when m <= (int)Speed.Good:
                    return Speed.Good;
                case var m when m > (int)Speed.Good && m <= (int)Speed.Acceptable:
                    return Speed.Acceptable;
                default:
                    return Speed.Bad;
            }
        }

        public enum Speed
        {
            Good = 60,
            Acceptable = 120,
            Bad = int.MaxValue
        }
    }
}
