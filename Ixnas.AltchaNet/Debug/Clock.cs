using System;

namespace Ixnas.AltchaNet.Debug
{
#if DEBUG
    public interface Clock
#else
    internal interface Clock
#endif
    {
        DateTimeOffset GetUtcNow();
    }
}
