using System;

namespace Ixnas.AltchaNet.Internal
{
    internal class DefaultClock : Clock
    {
        public DateTimeOffset GetUtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
