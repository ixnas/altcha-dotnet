using System;
using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class SaltTimestampValidator : ISaltValidator
    {
        private readonly int _expiryInSeconds;
        private readonly IJsonSerializer _serializer;

        public SaltTimestampValidator(IJsonSerializer serializer, int expiryInSeconds)
        {
            _serializer = serializer;
            _expiryInSeconds = expiryInSeconds;
        }

        public bool Validate(string salt)
        {
            var deserialized = _serializer.FromBase64Json<TimestampedSaltGenerator.TimestampedSalt>(salt);
            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(deserialized.T)
                                          .AddSeconds(_expiryInSeconds);
            // stryker disable once equality: Impossible to black box test to exactly now.
            return DateTimeOffset.UtcNow < timestamp;
        }
    }
}
