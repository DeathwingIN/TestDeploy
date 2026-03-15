using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroCloud.Licensing.Function.JsonConverters
{
    /// <summary>
    /// A flexible JSON converter that handles multiple date/time string formats  
    /// when deserialising API responses.
    /// </summary>
    public class FlexibleDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? raw = reader.GetString();
            return MicroCloud.Licensing.Function.Helpers.DateTimeConversionHelper.TryParseDateTime(raw);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(Helpers.DateTimeConversionHelper.ToIso8601(value.Value));
            else
                writer.WriteNullValue();
        }
    }
}
