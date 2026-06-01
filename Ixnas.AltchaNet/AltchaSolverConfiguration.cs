namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the configuration for the ALTCHA solver.
    /// </summary>
    public sealed record AltchaSolverConfiguration
    {
        /// <summary>
        ///     Disables checking for expiry before attempting to solve a challenge.
        /// </summary>
        public bool IgnoreExpiry { get; init; }
    }
}
