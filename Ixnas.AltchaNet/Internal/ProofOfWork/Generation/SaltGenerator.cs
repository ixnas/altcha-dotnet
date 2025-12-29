using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Common.Converters;
using Ixnas.AltchaNet.Internal.Common.Salt;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class SaltGenerator
    {
        private readonly Clock _clock;
        private readonly AltchaExpiry _expiry;

        public SaltGenerator(Clock clock,
                             AltchaExpiry expiry)
        {
            _clock = clock;
            _expiry = expiry;
        }

        public Salt Generate(AltchaExpiry? expiryOverride)
        {
            var expiry = expiryOverride ?? _expiry;
            var expiryUtc = _clock.UtcNow
                                  .AddSeconds(expiry.Seconds);
            var bytes = new byte[12];

            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var randomHexString = ByteConverter.GetHexStringFromBytes(bytes);
            var withExpiresParameter = $"{randomHexString}?expires={expiryUtc.ToUnixTimeSeconds()}&";
            return new Salt(_clock, withExpiresParameter, expiryUtc);
        }
    }
}
