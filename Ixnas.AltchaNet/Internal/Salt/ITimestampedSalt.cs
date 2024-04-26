using System;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal interface ITimestampedSalt
    {
        string ToBase64Json();
        DateTimeOffset GetExpiryUtc();
    }
}
