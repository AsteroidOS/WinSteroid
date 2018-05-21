using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class EnumExtensions
    {
        public static string GetRealValue(this Enum source)
        {
            if (source == null)
            {
                throw new ArgumentException(nameof(source));
            }

            var type = source.GetType();
            var member = type.GetMember(source.ToString()).Single();

            var displayAttribute = (DisplayAttribute)(member
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault());

            if (displayAttribute == null)
            {
                return source.ToString();
            }

            return displayAttribute.GetName();
        }
    }
}
