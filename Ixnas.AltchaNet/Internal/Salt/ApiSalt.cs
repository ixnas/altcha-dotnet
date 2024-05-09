using System;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class ApiSalt : TimestampedSalt
    {
        private readonly DateTimeOffset _expiryUtc;

        private ApiSalt(string salt)
        {
            _expiryUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(salt.Split('.')[0]));
        }

        public DateTimeOffset GetExpiryUtc()
        {
            return _expiryUtc;
        }

        public static Result<ApiSalt> Parse(string salt)
        {
            if (!long.TryParse(salt.Split('.')[0], out _))
                return new Result<ApiSalt>();

            return new Result<ApiSalt>
            {
                Success = true,
                Value = new ApiSalt(salt)
            };
        }
    }
}
