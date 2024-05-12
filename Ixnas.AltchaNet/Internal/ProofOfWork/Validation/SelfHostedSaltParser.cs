using System;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Common.Salt;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.ProofOfWork.Common;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Validation
{
    internal class SelfHostedSaltParser : SaltParser
    {
        private readonly Clock _clock;
        private readonly JsonSerializer _serializer;

        public SelfHostedSaltParser(JsonSerializer serializer, Clock clock)
        {
            _serializer = serializer;
            _clock = clock;
        }

        public Result<Salt> Parse(string salt)
        {
            if (salt == null)
                return new Result<Salt>();
            var deserializedResult =
                _serializer.FromBase64Json<SelfHostedSaltSerialized>(salt);
            if (!deserializedResult.Success)
                return new Result<Salt>();
            var deserialized = deserializedResult.Value;
            var expiryUtc = DateTimeOffset.FromUnixTimeMilliseconds(deserialized.T);
            return new Result<Salt>
            {
                Success = true,
                Value = new Salt(_clock, salt, expiryUtc)
            };
        }
    }
}
