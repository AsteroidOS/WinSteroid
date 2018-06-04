namespace WinSteroid.App.Messeges
{
    public class BatteryPercentageMessage
    {
        public int NewPercentage { get; private set; }

        public int OldPercentage { get; private set; }

        public static BatteryPercentageMessage Create(int newPercentage, int oldPercentage)
        {
            return new BatteryPercentageMessage
            {
                NewPercentage = newPercentage,
                OldPercentage = oldPercentage
            };
        }
    }
}
