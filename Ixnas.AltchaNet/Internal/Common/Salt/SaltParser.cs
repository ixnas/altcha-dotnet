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
            if (salt == null)
                return new Result<Salt>();
            var queryStringIndex = salt.IndexOf("?", StringComparison.Ordinal);
            if (queryStringIndex == -1)
                return new Result<Salt>();
            var queryString = HttpUtility.ParseQueryString(salt.Substring(queryStringIndex));
            var expires = queryString["expires"];
            if (!long.TryParse(expires, out var expiresParsed))
                return new Result<Salt>();

            var expiryUtc = DateTimeOffset.FromUnixTimeSeconds(expiresParsed);

            return new Result<Salt>
            {
                Success = true,
                Value = new Salt(_clock, salt, expiryUtc)
            };
        }
    }
}
