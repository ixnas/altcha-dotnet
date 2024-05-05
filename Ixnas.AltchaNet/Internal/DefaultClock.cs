using System;
using Ixnas.AltchaNet.Debug;

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
