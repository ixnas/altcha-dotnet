using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Debug;

namespace Ixnas.AltchaNet.Internal.Response
{
    internal class InMemoryStore : IAltchaChallengeStore
    {
        private class StoredChallenge
        {
            public string Challenge { get; set; }
            public DateTimeOffset ExpiryUtc { get; set; }
        }

        private readonly Clock _clock;
        private readonly List<StoredChallenge> _stored = new List<StoredChallenge>();

        public InMemoryStore(Clock clock)
        {
            _clock = clock;
        }

        public Task Store(string challenge, DateTimeOffset expiryUtc)
        {
            var challengeToStore = new StoredChallenge
            {
                Challenge = challenge,
                ExpiryUtc = expiryUtc
            };
            _stored.Add(challengeToStore);
            return Task.CompletedTask;
        }

        public Task<bool> Exists(string challenge)
        {
            // stryker disable once equality: Impossible to black box test to exactly now.
            _stored.RemoveAll(storedChallenge => storedChallenge.ExpiryUtc <= _clock.GetUtcNow());
            var exists = _stored.Exists(storedChallenge => storedChallenge.Challenge == challenge);
            return Task.FromResult(exists);
        }
    }
}
