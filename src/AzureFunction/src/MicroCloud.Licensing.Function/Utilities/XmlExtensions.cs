namespace Programmed.MAX.Function.Utilities
{
    public static class XmlExtensions
    {
        public static string EscapeXml(this string s)
        {
            if (s == null)
            {
                return s;
            }
            return s.Replace("&", "&amp;").Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;");
        }
    }
}
