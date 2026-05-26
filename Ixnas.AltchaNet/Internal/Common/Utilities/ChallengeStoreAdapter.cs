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
#if NET8_0_OR_GREATER
            return _challengeStore.Store(challenge, expiryUtc, cancellationToken);
#else
            return _challengeStore.Store(challenge, expiryUtc);
#endif
        }

        public Task<bool> Exists(string challenge, CancellationToken cancellationToken)
        {
#if NET8_0_OR_GREATER
            return _challengeStore.Exists(challenge, cancellationToken);
#else
            return _challengeStore.Exists(challenge);
#endif
        }
    }
}
