using System;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Cryptography;
using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class DefaultTimestampedSalt : TimestampedSalt
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

        public DefaultTimestampedSalt(JsonSerializer serializer,
                                      RandomNumberGenerator randomNumberGenerator,
                                      Clock clock,
                                      int expiryInSeconds)
        {
            _serializer = serializer;
            _expiryUtc = clock.GetUtcNow()
                              .AddSeconds(expiryInSeconds);
            _randomNumber = randomNumberGenerator.Generate(1000, 9999);
        }

        private DefaultTimestampedSalt(JsonSerializer serializer, int randomNumber, DateTimeOffset expiryUtc)
        {
            _serializer = serializer;
            _randomNumber = randomNumber;
            _expiryUtc = expiryUtc;
        }

        public DateTimeOffset GetExpiryUtc()
        {
            return _expiryUtc;
        }

        public static Result<DefaultTimestampedSalt> Parse(JsonSerializer serializer, string salt)
        {
            var deserializedResult =
                serializer.FromBase64Json<SerializedSalt>(salt);
            if (!deserializedResult.Success)
                return new Result<DefaultTimestampedSalt>();
            var deserialized = deserializedResult.Value;
            var randomNumber = deserialized.R;
            var expiryUtc = DateTimeOffset.FromUnixTimeMilliseconds(deserialized.T);
            return new Result<DefaultTimestampedSalt>
            {
                Success = true,
                Value = new DefaultTimestampedSalt(serializer, randomNumber, expiryUtc)
            };
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
    }
}
