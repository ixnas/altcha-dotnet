using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Cryptography;
using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class TimestampedSaltGenerator
    {
        private readonly Clock _clock;
        private readonly int _expiryInSeconds;
        private readonly RandomNumberGenerator _randomNumberGenerator;
        private readonly JsonSerializer _serializer;

        public TimestampedSaltGenerator(JsonSerializer serializer,
                                        RandomNumberGenerator randomNumberGenerator,
                                        Clock clock,
                                        int expiryInSeconds)
        {
            _serializer = serializer;
            _randomNumberGenerator = randomNumberGenerator;
            _clock = clock;
            _expiryInSeconds = expiryInSeconds;
        }

        public TimestampedSalt Generate()
        {
            return new TimestampedSalt(_serializer,
                                       _randomNumberGenerator,
                                       _clock,
                                       _expiryInSeconds);
        }
    }
}
