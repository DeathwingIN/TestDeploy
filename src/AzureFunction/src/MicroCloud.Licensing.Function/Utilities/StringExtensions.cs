using MicroCloud.Licensing.Function.Helpers;

namespace MicroCloud.Licensing.Function.Utilities
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string s1, string s2)
        {
            if (s1 == null && s2 == null)
            {
                return true;
            }
            else if (s1 == null || s2 == null)
            {
                return false;
            }

            return s1.Equals(s2, StringComparison.OrdinalIgnoreCase);
        }

        public static string Truncate(this string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Length <= maxLength ? input : input[..maxLength];
        }


    }
}
