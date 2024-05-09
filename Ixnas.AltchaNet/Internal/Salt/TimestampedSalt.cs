using System;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal interface TimestampedSalt
    {
        DateTimeOffset GetExpiryUtc();
    }
}
