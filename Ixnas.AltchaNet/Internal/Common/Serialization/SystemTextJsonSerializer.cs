using System;
using System.Text;
using System.Text.Json;

namespace Ixnas.AltchaNet.Internal.Common.Serialization
{
    internal class SystemTextJsonSerializer : JsonSerializer
    {
        private readonly static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Result<T> FromBase64Json<T>(string base64)
        {
            var altchaBytesResult = Base64ToBytes(base64);
            if (!altchaBytesResult.Success)
                return Result<T>.Fail(ErrorCode.ChallengeIsInvalidBase64);

            var altchaBytes = altchaBytesResult.Value;
            var altchaJson = BytesToString(altchaBytes);
            return JsonToObject<T>(altchaJson);
        }

        public string ToBase64Json<T>(T obj)
        {
            var serialized = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj, SerializerOptions);
            return Convert.ToBase64String(serialized);
        }

        private static Result<byte[]> Base64ToBytes(string base64)
        {
            try
            {
                return Result<byte[]>.Ok(Convert.FromBase64String(base64));
            }
            catch (FormatException)
            {
                return Result<byte[]>.Fail();
            }
        }

        private static string BytesToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        private static Result<T> JsonToObject<T>(string json)
        {
            try
            {
                return Result<T>.Ok(System.Text.Json.JsonSerializer.Deserialize<T>(json, SerializerOptions));
            }
            catch (JsonException)
            {
                return Result<T>.Fail(ErrorCode.ChallengeIsInvalidJson);
            }
        }
    }
}
