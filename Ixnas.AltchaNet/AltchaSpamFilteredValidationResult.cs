namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the result of validating a spam filtered ALTCHA form.
    /// </summary>
    public sealed class AltchaSpamFilteredValidationResult
    {
        /// <summary>
        ///     Whether the validation was succesful.
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        ///     Whether the form successfully passed through the spam filter.
        /// </summary>
        public bool PassedSpamFilter { get; set; }
    }
}
