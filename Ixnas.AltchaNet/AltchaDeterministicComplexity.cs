namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the deterministic complexity for generating ALTCHA challenges.
    ///     Tweaks the computational effort required to solve a challenge.
    /// </summary>
#if NET8_0_OR_GREATER
    public sealed record AltchaDeterministicComplexity
    {
        /// <summary>
        /// (Required) Initializes the counter range.
        /// </summary>
        public required AltchaComplexityCounterRange Counter { get; init; }

        /// <summary>
        /// (Required) Initializes the cost.
        /// </summary>
        public required int Cost { get; init; } 
    }
#else
    public sealed class AltchaDeterministicComplexity
    {
        /// <summary>
        ///     (Required) Initializes the counter range.
        /// </summary>
        public AltchaComplexityCounterRange Counter { get; set; }
        /// <summary>
        ///     (Required) Initializes the cost.
        /// </summary>
        public int Cost { get; set; }
    }
#endif
}
