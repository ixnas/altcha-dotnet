using System;
using Ixnas.AltchaNet.Internal;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the ALTCHA configuration for the SHA-256 algorithm.
    /// </summary>
    public sealed record AltchaSha256Configuration
    {
        /// <summary>
        /// (Required) The key to use to generate and validate challenges.
        /// </summary>
        public required AltchaKey Key { get; init; }
        
        /// <summary>
        ///     (Required) A store to use for previously verified ALTCHA responses. Used to prevent replay attacks.
        /// </summary>
        public required Func<IAltchaChallengeStore> StoreFactory { get; init; }
        
        /// <summary>
        ///     (Optional) Overrides the default time it takes for a challenge to expire.
        /// </summary>
        public AltchaExpiry Expiry { get; init; } = AltchaExpiry.FromSeconds(Defaults.ExpiryInSeconds);
        
        /// <summary>
        ///     (Optional) Overrides the default complexity to tweak the amount of computational effort a client has to put in.
        ///     See https://playground.altcha.org/#/about for more information
        /// </summary>
        public AltchaDeterministicComplexity Complexity { get; init; } = new()
        {
            Counter = new AltchaComplexityCounterRange(50_000, 100_000),
            Cost = 1,
        };
    }
}
