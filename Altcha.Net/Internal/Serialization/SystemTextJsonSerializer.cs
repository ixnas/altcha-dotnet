using System;
using System.Text;
using System.Text.Json;

namespace Altcha.Net.Internal.Serialization
{
    internal class SystemTextJsonSerializer : IJsonSerializer
    {
        public T FromBase64Json<T>(string base64)
        {
            var altchaBytes = Convert.FromBase64String(base64);
            var altchaJson = Encoding.UTF8.GetString(altchaBytes);
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<T>(altchaJson, serializerOptions);
        }
    }
}
