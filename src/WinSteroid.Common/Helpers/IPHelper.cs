using System;
using System.Linq;

namespace WinSteroid.Common.Helpers
{
    public class IPHelper
    {
        public static bool IsValid(string hostIp)
        {
            if (string.IsNullOrWhiteSpace(hostIp)) return false;

            var segments = hostIp.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.IsNullOrEmpty() || segments.Length != 4) return false;

            foreach (var segment in segments)
            {
                if (!byte.TryParse(segment, out byte n)) return false;
            }

            return true;
        }
    }
}
