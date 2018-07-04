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

namespace WinSteroid.Shared.Models
{
    public enum WeatherID
    {
        //THUNDERSTORM 2xx
        ThunderStormWithLightRain = 200,
        ThunderStormWithRain = 201,
        ThunderStormWithHeavyRain = 202,
        LigthThunderStorm = 210,
        ThunderStorm = 211,
        HeavyThunderStorm = 212,
        RaggedThunderStormWith = 221,
        ThunderStormWithLightDrizzle = 230,
        ThunderStormWithDrizzle = 231,
        ThunderStormWithHeavyDrizzle = 232,
        //DRIZZLE 3xx
        LightIntensityDrizzle = 300,
        Drizzle = 301,
        HeavyIntensityDrizzle = 302,
        LightIntensityDrizzleRain = 310,
        DrizzleRain = 311,
        HeavyIntensityDrizzleRain = 312,
        ShowerRainAndDrizzle = 313,
        HeavyShowerRainAndDrizzle = 314,
        ShowerDrizzle = 321,
        //RAIN 5xx
        LightRain = 500,
        ModerateRain = 501,
        HeavyIntensityRain = 502,
        VeryHeavyRain = 503,
        ExtremeRain = 504,
        FreezingRain = 511,
        LightIntensityShowerRain = 520,
        ShowerRain = 521,
        HeavyIntensityShowerRain = 522,
        RaggedShowerRain = 531,
        //SNOW 6xx
        LightSnow = 600,
        Snow = 601,
        HeavySnow = 602,
        Sleet = 611,
        ShowerSleet = 612,
        LightRainAndSnow = 615,
        RainAndSnow = 616,
        LightShowerSnow = 620,
        ShowerSnow = 621,
        HeavyShowerSnow = 622,
        //ATMOSPHERE 7xx
        Mist = 701,
        Smoke = 711,
        Haze = 721,
        SandDustWhirls = 731,
        Fog = 741,
        Sand = 751,
        Dust = 761,
        VolcanicAsh = 762,
        Squalls = 771,
        Tornado = 781,
        //CLEAR & CLOUDS 8xx
        ClearSky = 800,
        FewClouds = 801,
        ScatteredClouds = 802,
        BrokenClouds = 803,
        OvercastClouds = 804
    }
}
