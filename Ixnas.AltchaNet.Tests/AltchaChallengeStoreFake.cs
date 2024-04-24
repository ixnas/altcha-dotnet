namespace Ixnas.AltchaNet.Tests;

internal class AltchaChallengeStoreFake : IAltchaChallengeStore
{
    public Task Store(string challenge)
    {
        return Task.CompletedTask;
    }

    public Task<bool> Exists(string challenge)
    {
        return Task.FromResult(false);
    }
}
