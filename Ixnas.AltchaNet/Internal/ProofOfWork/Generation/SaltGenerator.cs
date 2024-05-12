using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Common.Salt;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.ProofOfWork.Common;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class SaltGenerator
    {
        private readonly Clock _clock;
        private readonly int _expiryInSeconds;
        private readonly RandomNumberGenerator _randomNumberGenerator;
        private readonly JsonSerializer _serializer;

        public SaltGenerator(JsonSerializer serializer,
                             RandomNumberGenerator randomNumberGenerator,
                             Clock clock,
                             int expiryInSeconds)
        {
            _serializer = serializer;
            _randomNumberGenerator = randomNumberGenerator;
            _clock = clock;
            _expiryInSeconds = expiryInSeconds;
        }

        public Salt Generate()
        {
            var expiryUtc = _clock.UtcNow
                                  .AddSeconds(_expiryInSeconds);
            var randomNumber = _randomNumberGenerator.Generate();
            var serialized = new SelfHostedSaltSerialized
            {
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                T = expiryUtc.ToUnixTimeMilliseconds(),
                R = randomNumber
            };
            var raw = _serializer.ToBase64Json(serialized);
            return new Salt(_clock, raw, expiryUtc);
        }
    }
}
