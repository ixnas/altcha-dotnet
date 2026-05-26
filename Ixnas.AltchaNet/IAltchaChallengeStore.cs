using System;
using System.Threading.Tasks;
#if NET8_0_OR_GREATER
using System.Threading;
#endif

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents a data source to store challenges that have been solved before.
    ///     Is used to prevent replay attacks.
    /// </summary>
    public interface IAltchaChallengeStore
    {
        /// <summary>
        ///     Stores a challenge after it's been verified.
        /// </summary>
        /// <param name="challenge">String representation of the verified challenge.</param>
        /// <param name="expiryUtc">
        ///     Timestamp after which the challenge expires. Can be used to periodically remove all expired
        ///     challenges from the store.
        /// </param>
#if NET8_0_OR_GREATER
        [Obsolete("Will be removed from this interface in the next major version. Make sure to implement Exists(string, DateTimeOffset, CancellationToken) instead.")]
#endif
        Task Store(string challenge, DateTimeOffset expiryUtc);

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Stores a challenge after it's been verified.
        /// </summary>
        /// <param name="challenge">String representation of the verified challenge.</param>
        /// <param name="expiryUtc">
        ///     Timestamp after which the challenge expires. Can be used to periodically remove all expired
        ///     challenges from the store.
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// </param>
        Task Store(string challenge, DateTimeOffset expiryUtc, CancellationToken cancellationToken)
        {
            return Store(challenge, expiryUtc);
        }
#endif

        /// <summary>
        ///     Checks if a challenge has been stored before.
        /// </summary>
        /// <param name="challenge">String representation of the challenge.</param>
        /// <returns>true if it exists, false if it doesn't.</returns>
#if NET8_0_OR_GREATER
        [Obsolete("Will be removed from this interface in the next major version. Make sure to implement Exists(string, CancellationToken) instead.")]
#endif
        Task<bool> Exists(string challenge);

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Checks if a challenge has been stored before.
        /// </summary>
        /// <param name="challenge">String representation of the challenge.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>true if it exists, false if it doesn't.</returns>
        Task<bool> Exists(string challenge, CancellationToken cancellationToken)
        {
            return Exists(challenge);
        }
#endif
    }
}
