using System;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Common.Salt;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Validation
{
    internal class ApiSaltParser : SaltParser
    {
        private readonly Clock _clock;

        public ApiSaltParser(Clock clock)
        {
            _clock = clock;
        }

        public Result<Salt> Parse(string salt)
        {
            if (salt == null)
                return new Result<Salt>();
            if (!long.TryParse(salt.Split('.')[0], out _))
                return new Result<Salt>();

            var expiryUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(salt.Split('.')[0]));

            return new Result<Salt>
            {
                Success = true,
                Value = new Salt(_clock, salt, expiryUtc)
            };
        }
    }
}
