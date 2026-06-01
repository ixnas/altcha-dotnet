namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the deterministic complexity for generating ALTCHA challenges.
    ///     Tweaks the computational effort required to solve a challenge.
    /// </summary>
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
}
