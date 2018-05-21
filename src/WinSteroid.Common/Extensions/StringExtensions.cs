namespace System
{
    public static class StringExtensions
    {
        public static bool OrdinalIgnoreCaseEquals(string string1, string string2)
        {
            return string.Equals(string1, string2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
