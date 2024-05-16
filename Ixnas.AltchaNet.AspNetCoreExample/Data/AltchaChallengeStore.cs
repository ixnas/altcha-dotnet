using Microsoft.EntityFrameworkCore;

namespace Ixnas.AltchaNet.AspNetCoreExample.Data;

internal class AltchaChallengeStore : IAltchaChallengeStore
{
    private readonly ExampleDbContext _dbContext;

    public AltchaChallengeStore(ExampleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Store(string challenge, DateTimeOffset expiryUtc)
    {
        var verifiedChallenge = new VerifiedChallenge
        {
            Challenge = challenge,
            ExpiryUtc = expiryUtc.UtcDateTime
        };
        _dbContext.VerifiedChallenges.Add(verifiedChallenge);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> Exists(string challenge)
    {
        var expiredChallenges = _dbContext.VerifiedChallenges.Where(storedChallenge =>
                                                                        storedChallenge.ExpiryUtc
                                                                        <= DateTime.UtcNow);
        _dbContext.RemoveRange(expiredChallenges);
        await _dbContext.SaveChangesAsync();
        return await _dbContext
                     .VerifiedChallenges
                     .AnyAsync(storedChallenge =>
                                   storedChallenge.Challenge == challenge);
    }
}
