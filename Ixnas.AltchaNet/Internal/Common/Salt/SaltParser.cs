using System;
using System.Web;
using Ixnas.AltchaNet.Debug;

namespace Ixnas.AltchaNet.Internal.Common.Salt
{
    internal class SaltParser
    {
        private readonly Clock _clock;

        public SaltParser(Clock clock)
        {
            _clock = clock;
        }

        public Result<Salt> Parse(string salt)
        {
            var errorCode = ErrorCode.InvalidSalt;
            if (salt == null)
                return Result<Salt>.Fail(errorCode);
            var queryStringIndex = salt.IndexOf("?", StringComparison.Ordinal);
            if (queryStringIndex == -1)
                return Result<Salt>.Fail(errorCode);
            var queryString = HttpUtility.ParseQueryString(salt.Substring(queryStringIndex));
            var expires = queryString["expires"];
            if (!long.TryParse(expires, out var expiresParsed))
                return Result<Salt>.Fail(errorCode);

            var expiryUtc = DateTimeOffset.FromUnixTimeSeconds(expiresParsed);

            return Result<Salt>.Ok(new Salt(_clock, salt, expiryUtc));
        }
    }
}
