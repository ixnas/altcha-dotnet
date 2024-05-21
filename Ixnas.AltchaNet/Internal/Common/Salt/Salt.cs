using System;
using Ixnas.AltchaNet.Debug;

namespace Ixnas.AltchaNet.Internal.Common.Salt
{
    internal class Salt
    {
        public DateTimeOffset ExpiryUtc { get; }
        public string Raw { get; }
        private readonly Clock _clock;

        public Salt(Clock clock, string salt, DateTimeOffset expiryUtc)
        {
            _clock = clock;
            Raw = salt;
            ExpiryUtc = expiryUtc;
        }

        public bool HasExpired()
        {
            // stryker disable once equality: Impossible to black box test to exactly now.
            return ExpiryUtc <= _clock.UtcNow;
        }
    }
}
