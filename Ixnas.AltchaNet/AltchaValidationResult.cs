namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the result of validating an ALTCHA challenge response.
    /// </summary>
    public sealed class AltchaValidationResult
    {
        /// <summary>
        ///     Whether the response was valid.
        /// </summary>
        public bool IsValid { get; set; }
    }
}
