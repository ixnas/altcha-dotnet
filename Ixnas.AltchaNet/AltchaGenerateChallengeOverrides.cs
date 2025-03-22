namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Allows overriding configuration options when generating ALTCHA challenges.
    /// </summary>
    public sealed class AltchaGenerateChallengeOverrides
    {
        /// <summary>
        ///     Overrides the configured complexity.
        /// </summary>
        public AltchaComplexity? Complexity { get; set; }
        /// <summary>
        ///     Overrides the configured expiry.
        /// </summary>
        public AltchaExpiry? Expiry { get; set; }
    }
}
