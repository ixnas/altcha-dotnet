using System;
using System.Text;
using System.Text.Json;

namespace Ixnas.AltchaNet.Internal.Serialization
{
    internal class SystemTextJsonSerializer : JsonSerializer
    {
        public T FromBase64Json<T>(string base64)
        {
            var altchaBytes = Convert.FromBase64String(base64);
            var altchaJson = Encoding.UTF8.GetString(altchaBytes);
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return System.Text.Json.JsonSerializer.Deserialize<T>(altchaJson, serializerOptions);
        }

        public string ToBase64Json<T>(T obj)
        {
            var serialized = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj);
            return Convert.ToBase64String(serialized);
        }
    }
}
