namespace Altcha.Net.Internal.Response
{
    internal class ValidationResult : IAltchaValidationResult
    {
        public bool IsValid { get; internal set; }
    }
}
