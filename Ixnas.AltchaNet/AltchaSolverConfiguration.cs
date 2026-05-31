namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the configuration for the ALTCHA solver.
    /// </summary>
#if NET8_0_OR_GREATER
    public sealed record AltchaSolverConfiguration
    {
        /// <summary>
        ///     Disables checking for expiry before attempting to solve a challenge.
        /// </summary>
        public bool IgnoreExpiry { get; init; }
    }
#else
    public sealed class AltchaSolverConfiguration
    {
        /// <summary>
        ///     Disables checking for expiry before attempting to solve a challenge.
        /// </summary>
        public bool IgnoreExpiry { get; set; }
    }
#endif
}
