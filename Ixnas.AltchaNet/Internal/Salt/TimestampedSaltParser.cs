using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class TimestampedSaltParser : ITimestampedSaltParser
    {
        private readonly IJsonSerializer _serializer;

        public TimestampedSaltParser(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public ITimestampedSalt FromBase64Json(string salt)
        {
            return new TimestampedSalt(_serializer, salt);
        }
    }
}
