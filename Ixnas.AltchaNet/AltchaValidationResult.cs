using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the result of validating an ALTCHA challenge response.
    /// </summary>
    [DataContract]
    public sealed class AltchaValidationResult
    {
        /// <summary>
        ///     Whether the response was valid.
        /// </summary>
        [JsonPropertyName("isValid")]
        [DataMember(Name = "isValid")]
        public bool IsValid { get; set; }
        /// <summary>
        ///     Challenge validation error.
        /// </summary>
        [JsonPropertyName("validationError")]
        [DataMember(Name = "validationError")]
        public AltchaValidationError ValidationError { get; set; }
    }
}
