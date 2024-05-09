namespace Ixnas.AltchaNet.Tests.Fakes;

internal class AltchaChallengeStoreFake : IAltchaChallengeStore
{
    public (string Challenge, DateTimeOffset Expiry)? Stored { get; private set; }

    public Task Store(string challenge, DateTimeOffset expiryUtc)
    {
        Stored = new ValueTuple<string, DateTimeOffset>(challenge, expiryUtc);
        return Task.CompletedTask;
    }

    public Task<bool> Exists(string challenge)
    {
        return Task.FromResult(false);
    }
}
