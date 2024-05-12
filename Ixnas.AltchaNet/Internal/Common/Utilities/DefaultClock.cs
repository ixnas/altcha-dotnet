using System;
using Ixnas.AltchaNet.Debug;

namespace Ixnas.AltchaNet.Internal.Common.Utilities
{
    internal class DefaultClock : Clock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
