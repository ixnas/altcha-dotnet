using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Debug;

namespace Ixnas.AltchaNet.Tests.Fakes
{
    internal class InMemoryStore : IAltchaChallengeStore
    {
        private class StoredChallenge
        {
            public string Challenge { get; init; }
            public DateTimeOffset ExpiryUtc { get; init; }
        }

        private readonly Clock _clock;
        private readonly List<StoredChallenge> _stored = [];

        public InMemoryStore(Clock clock)
        {
            _clock = clock;
        }

        public Task Store(string challenge, DateTimeOffset expiryUtc, CancellationToken cancellationToken)
        {
            var challengeToStore = new StoredChallenge
            {
                Challenge = challenge,
                ExpiryUtc = expiryUtc
            };
            _stored.Add(challengeToStore);
            return Task.CompletedTask;
        }

        public Task<bool> Exists(string challenge, CancellationToken cancellationToken)
        {
            // stryker disable once equality: Impossible to black box test to exactly now.
            _stored.RemoveAll(storedChallenge => storedChallenge.ExpiryUtc <= _clock.UtcNow);
            var exists = _stored.Exists(storedChallenge => storedChallenge.Challenge == challenge);
            return Task.FromResult(exists);
        }
    }
}
