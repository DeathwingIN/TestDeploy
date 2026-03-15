using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MicroCloud.Licensing.Function.Helpers
{
    public static class XmlHelper
    {
        public static T Deserialize<T>(string input) where T : class
        {

            XmlSerializer ser = new(typeof(T));

            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (input.StartsWith(_byteOrderMarkUtf8, StringComparison.Ordinal)) {
                input = input.Remove(0, _byteOrderMarkUtf8.Length);
            }
            using StringReader sr = new(input);
            return (T)ser.Deserialize(sr);
        }

        public static string Serialize<T>(T ObjectToSerialize)
        {

            XmlSerializer xmlSerializer = new(ObjectToSerialize.GetType());

            using StringWriter textWriter = new();
            xmlSerializer.Serialize(textWriter, ObjectToSerialize);
            return textWriter.ToString();
        }
    }
}
