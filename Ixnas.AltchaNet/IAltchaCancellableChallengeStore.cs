using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents a cancellable data source to store challenges that have been solved before.
    ///     Is used to prevent replay attacks.
    /// </summary>
    public interface IAltchaCancellableChallengeStore
    {
        /// <summary>
        ///     Stores a challenge after it's been verified.
        /// </summary>
        /// <param name="challenge">String representation of the verified challenge.</param>
        /// <param name="expiryUtc">
        ///     Timestamp after which the challenge expires. Can be used to periodically remove all expired
        ///     challenges from the store.
        /// </param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        Task Store(string challenge, DateTimeOffset expiryUtc, CancellationToken cancellationToken);

        /// <summary>
        ///     Checks if a challenge has been stored before.
        /// </summary>
        /// <param name="challenge">String representation of the challenge.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>true if it exists, false if it doesn't.</returns>
        Task<bool> Exists(string challenge, CancellationToken cancellationToken);
    }
}
