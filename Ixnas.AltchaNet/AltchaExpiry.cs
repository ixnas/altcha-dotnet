using Ixnas.AltchaNet.Exceptions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the time after which a generated challenge expires.
    /// </summary>
    [DataContract]
    public readonly struct AltchaExpiry
    {
        /// <summary>
        ///     Expiry in seconds.
        /// </summary>
        [JsonPropertyName("seconds")]
        [DataMember(Name = "seconds")]
        public int Seconds { get; }

        private AltchaExpiry(int seconds)
        {
            if (seconds < 1)
                throw new InvalidExpiryException();
            Seconds = seconds;
        }

        /// <summary>
        ///     Constructs expiry from a specified number of seconds.
        /// </summary>
        /// <param name="seconds">Number of seconds after which the challenge should expire.</param>
        /// <exception cref="InvalidExpiryException">Thrown when attempting to set a negative expiry.</exception>
        /// <returns>A new AltchaExpiry instance.</returns>
        public static AltchaExpiry FromSeconds(int seconds)
        {
            return new AltchaExpiry(seconds);
        }
    }
}
