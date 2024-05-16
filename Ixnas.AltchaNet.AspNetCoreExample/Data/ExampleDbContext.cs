using Microsoft.EntityFrameworkCore;

namespace Ixnas.AltchaNet.AspNetCoreExample.Data;

internal class ExampleDbContext : DbContext
{
    public DbSet<VerifiedChallenge> VerifiedChallenges { get; set; }

    public ExampleDbContext(DbContextOptions options) : base(options)
    {
    }
}
