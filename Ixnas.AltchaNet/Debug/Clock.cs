using System;

namespace Ixnas.AltchaNet
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
