using System;

namespace Ixnas.AltchaNet.Debug
{
#if DEBUG
    /// <summary>
    ///     Abstraction for getting the current time.
    /// </summary>
    public interface Clock
#else
    internal interface Clock
#endif
    {
        /// <summary>
        ///     Returns the current UTC time.
        /// </summary>
        DateTimeOffset GetUtcNow();
    }
}
