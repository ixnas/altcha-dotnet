using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class TimestampedSaltParser
    {
        private readonly JsonSerializer _serializer;

        public TimestampedSaltParser(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public TimestampedSalt FromBase64Json(string salt)
        {
            return new TimestampedSalt(_serializer, salt);
        }
    }
}
