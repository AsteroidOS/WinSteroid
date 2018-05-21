using System;
using System.Collections.Generic;
using System.Linq;

namespace WinSteroid.Common.Models
{
    public static class ApplicationIconExtensions
    {
        public static List<ApplicationIcon> GetList()
        {
            return Enum.GetValues(typeof(ApplicationIcon))
                .Cast<ApplicationIcon>()
                .OrderBy(ai => (int)ai)
                .ToList();
        }

        public static IDictionary<string, string> GetDictionary()
        {
            return GetList()
                .Select(ai => new KeyValuePair<string, string>(ai.GetRealValue(), ai.ToString()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
