using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ixnas.AltchaNet.Tests.Fakes
{
    internal class AltchaChallengeStoreFake : IAltchaChallengeStore
    {
        public CancellationMethod CancellationSimulation { get; init; } = CancellationMethod.None;
        public (string Challenge, DateTimeOffset Expiry)? Stored { get; private set; }

        public Task Store(string challenge, DateTimeOffset expiryUtc, CancellationToken cancellationToken)
        {
            if (CancellationSimulation != CancellationMethod.Store)
            {
                Stored = new ValueTuple<string, DateTimeOffset>(challenge, expiryUtc);
                return Task.CompletedTask;
            }
            while (!cancellationToken.IsCancellationRequested)
            {
            }

            throw new OperationCanceledException();
        }

        public Task<bool> Exists(string challenge, CancellationToken cancellationToken)
        {
            if (CancellationSimulation != CancellationMethod.Exists)
                return Task.FromResult(false);
            while (!cancellationToken.IsCancellationRequested)
            {
            }

            throw new OperationCanceledException();
        }
    }

    public enum CancellationMethod
    {
        None,
        Store,
        Exists
    }
}
