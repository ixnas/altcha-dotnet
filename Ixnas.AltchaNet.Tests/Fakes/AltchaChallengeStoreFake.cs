using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ixnas.AltchaNet.Tests.Fakes
{
    internal class AltchaChallengeStoreFake : IAltchaChallengeStore, IAltchaCancellableChallengeStore
    {
        public CancellationMethod CancellationSimulation { get; set; } = CancellationMethod.None;
        public (string Challenge, DateTimeOffset Expiry)? Stored { get; private set; }

        public Task Store(string challenge, DateTimeOffset expiryUtc, CancellationToken cancellationToken)
        {
            if (CancellationSimulation != CancellationMethod.Store)
                return Store(challenge, expiryUtc);
            while (!cancellationToken.IsCancellationRequested)
            {
            }

            throw new OperationCanceledException();
        }

        public Task<bool> Exists(string challenge, CancellationToken cancellationToken)
        {
            if (CancellationSimulation != CancellationMethod.Exists)
                return Exists(challenge);
            while (!cancellationToken.IsCancellationRequested)
            {
            }

            throw new OperationCanceledException();
        }

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

    public enum CancellationMethod
    {
        None,
        Store,
        Exists
    }
}
