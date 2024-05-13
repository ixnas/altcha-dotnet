using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Common.Converters;
using Ixnas.AltchaNet.Internal.Common.Salt;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class SaltGenerator
    {
        private readonly Clock _clock;
        private readonly int _expiryInSeconds;

        public SaltGenerator(Clock clock,
                             int expiryInSeconds)
        {
            _clock = clock;
            _expiryInSeconds = expiryInSeconds;
        }

        public Salt Generate()
        {
            var expiryUtc = _clock.UtcNow
                                  .AddSeconds(_expiryInSeconds);
            var bytes = new byte[12];

            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var randomHexString = ByteConverter.GetHexStringFromBytes(bytes);
            var withExpiresParameter = $"{randomHexString}?expires={expiryUtc.ToUnixTimeSeconds()}";
            return new Salt(_clock, withExpiresParameter, expiryUtc);
        }
    }
}
