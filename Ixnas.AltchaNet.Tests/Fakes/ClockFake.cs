using Ixnas.AltchaNet.Debug;

namespace Ixnas.AltchaNet.Tests.Fakes;

internal class ClockFake : Clock
{
    private int _offsetInSeconds;
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow.AddSeconds(_offsetInSeconds);

    public void SetOffsetInSeconds(int offsetInSeconds)
    {
        _offsetInSeconds = offsetInSeconds;
    }
}
