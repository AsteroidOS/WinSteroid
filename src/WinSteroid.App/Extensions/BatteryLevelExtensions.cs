namespace WinSteroid.App.Models
{
    public static class BatteryLevelExtensions
    {
        public static BatteryLevel Parse(ushort percentage)
        {
            switch (percentage)
            {
                case var p when percentage > (int)BatteryLevel.Discrete:
                    {
                        return BatteryLevel.Good;
                    }
                case var p when percentage <= (int)BatteryLevel.Discrete && percentage > (int)BatteryLevel.Bad:
                    {
                        return BatteryLevel.Discrete;
                    }
                case var p when percentage <= (int)BatteryLevel.Bad && percentage > (int)BatteryLevel.Critic:
                    {
                        return BatteryLevel.Bad;
                    }
            }

            return BatteryLevel.Critic;
        }
    }
}
