using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class TimestampedSaltGenerator : ITimestampedSaltGenerator
    {
        private readonly int _expiryInSeconds;
        private readonly IJsonSerializer _serializer;

        public TimestampedSaltGenerator(IJsonSerializer serializer, int expiryInSeconds)
        {
            _serializer = serializer;
            _expiryInSeconds = expiryInSeconds;
        }

        public ITimestampedSalt Generate()
        {
            return new TimestampedSalt(_serializer, _expiryInSeconds);
        }
    }
}
