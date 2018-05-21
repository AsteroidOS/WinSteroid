namespace System.Text
{
    public static class StringBuilderExtensions
    {
        public static void AppendNode(this StringBuilder stringBuilder, string name, string value)
        {
            stringBuilder.Append("<" + name + ">");
            stringBuilder.Append(value);
            stringBuilder.Append("</" + name + ">");
        }
    }
}
