using System;
using System.Globalization;

namespace MicroCloud.Licensing.Function.Helpers
{
    public static class DateTimeConversionHelper
    {
        private static readonly string[] DateTimeFormats =
        {
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy HH:mm",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-dd",
            "MM/dd/yyyy",
            "M/d/yyyy",
        };

        /// <summary>
        /// Parses a date/time string using multiple known formats. Returns null if unparseable.
        /// </summary>
        public static DateTime? TryParseDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (DateTime.TryParseExact(value, DateTimeFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
            {
                return dt;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dtFallback))
            {
                return dtFallback;
            }

            return null;
        }

        /// <summary>
        /// Formats a DateTime to ISO 8601 UTC string.
        /// </summary>
        public static string ToIso8601(DateTime dt) =>
            dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }
}
