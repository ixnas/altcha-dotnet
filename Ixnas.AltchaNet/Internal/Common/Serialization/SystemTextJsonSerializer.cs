using System;
using System.Text;
using System.Text.Json;

namespace Ixnas.AltchaNet.Internal.Common.Serialization
{
    internal class SystemTextJsonSerializer : JsonSerializer
    {
        public Result<T> FromBase64Json<T>(string base64)
        {
            var altchaBytesResult = Base64ToBytes(base64);
            if (!altchaBytesResult.Success)
                return new Result<T>();

            var altchaBytes = altchaBytesResult.Value;
            var altchaJson = BytesToString(altchaBytes);
            return JsonToObject<T>(altchaJson);
        }

        public string ToBase64Json<T>(T obj)
        {
            var serialized = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj);
            return Convert.ToBase64String(serialized);
        }

        private static Result<byte[]> Base64ToBytes(string base64)
        {
            try
            {
                return new Result<byte[]>
                {
                    Success = true,
                    Value = Convert.FromBase64String(base64)
                };
            }
            catch (FormatException)
            {
                return new Result<byte[]>();
            }
        }

        private static string BytesToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        private static Result<T> JsonToObject<T>(string json)
        {
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            try
            {
                return new Result<T>
                {
                    Success = true,
                    Value = System.Text.Json.JsonSerializer.Deserialize<T>(json, serializerOptions)
                };
            }
            catch (JsonException)
            {
                return new Result<T>();
            }
        }
    }
}
