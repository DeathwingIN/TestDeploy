using Newtonsoft.Json;
using System.IO;

namespace MicroCloud.Licensing.Function.Utilities
{
    public class JsonHelper
    {
        public static void Serialize(object value, Stream s)
        {
            using var writer = new StreamWriter(s);
            using var jsonWriter = new JsonTextWriter(writer);
            var ser = new JsonSerializer();
            ser.Serialize(jsonWriter, value);
            jsonWriter.Flush();
        }

        public static T Deserialize<T>(Stream s)
        {
            using var reader = new StreamReader(s);
            string body = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(body);
        }
    }
}
