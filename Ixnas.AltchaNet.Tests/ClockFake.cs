using Ixnas.AltchaNet.Debug;

namespace Ixnas.AltchaNet.Tests;

internal class ClockFake : Clock
{
    private int _offsetInSeconds;

    public DateTimeOffset GetUtcNow()
    {
        return DateTimeOffset.UtcNow.AddSeconds(_offsetInSeconds);
    }

    public void SetOffsetInSeconds(int offsetInSeconds)
    {
        _offsetInSeconds = offsetInSeconds;
    }
}
