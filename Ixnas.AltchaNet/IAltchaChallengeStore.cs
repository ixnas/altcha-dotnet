using System.Threading.Tasks;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents a data source to store challenges that have been solved before. Is used to prevent replay attacks.
    /// </summary>
    public interface IAltchaChallengeStore
    {
        /// <summary>
        ///     Stores a challenge after it's been verified.
        /// </summary>
        /// <param name="challenge">String representation of the verified challenge.</param>
        Task Store(string challenge);

        /// <summary>
        ///     Checks if a challenge has been stored before.
        /// </summary>
        /// <param name="challenge">String representation of the challenge.</param>
        /// <returns>true if it exists, false if it doesn't</returns>
        Task<bool> Exists(string challenge);
    }
}
