using System;
using System.Threading;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Exceptions;

namespace Ixnas.AltchaNet.Internal.Common.Utilities
{
    internal class ChallengeStoreAdapter : IAltchaCancellableChallengeStore
    {
        private readonly IAltchaChallengeStore _challengeStore;

        public ChallengeStoreAdapter(IAltchaChallengeStore challengeStore)
        {
            Guard.NotNull<MissingStoreException>(challengeStore);
            _challengeStore = challengeStore;
        }

        public Task Store(string challenge, DateTimeOffset expiryUtc, CancellationToken cancellationToken)
        {
            return _challengeStore.Store(challenge, expiryUtc);
        }

        public Task<bool> Exists(string challenge, CancellationToken cancellationToken)
        {
            return _challengeStore.Exists(challenge);
        }
    }
}
