using System;
using Ixnas.AltchaNet.Internal.Cryptography;
using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class TimestampedSalt
    {
        [Serializable]
        private class SerializedSalt
        {
            public long T { get; set; } // Timestamp
            public int R { get; set; } // Random number
        }

        private readonly DateTimeOffset _expiryUtc;
        private readonly int _randomNumber;
        private readonly JsonSerializer _serializer;

        public TimestampedSalt(JsonSerializer serializer,
                               RandomNumberGenerator randomNumberGenerator,
                               Clock clock,
                               int expiryInSeconds)
        {
            _serializer = serializer;
            _expiryUtc = clock.GetUtcNow()
                              .AddSeconds(expiryInSeconds);
            _randomNumber = randomNumberGenerator.Generate(1000, 9999);
        }

        public TimestampedSalt(JsonSerializer serializer, string salt)
        {
            _serializer = serializer;
            var deserialized =
                _serializer.FromBase64Json<SerializedSalt>(salt);
            _randomNumber = deserialized.R;
            _expiryUtc = DateTimeOffset.FromUnixTimeMilliseconds(deserialized.T);
        }

        public string ToBase64Json()
        {
            var timestampedSalt = new SerializedSalt
            {
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                T = _expiryUtc.ToUnixTimeMilliseconds(),
                R = _randomNumber
            };
            return _serializer.ToBase64Json(timestampedSalt);
        }

        public DateTimeOffset GetExpiryUtc()
        {
            return _expiryUtc;
        }
    }
}
