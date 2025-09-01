using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the result of validating a spam filtered ALTCHA form.
    /// </summary>
    [DataContract]
    public sealed class AltchaSpamFilteredValidationResult
    {
        /// <summary>
        ///     Whether the validation was successful.
        /// </summary>
        [JsonPropertyName("isValid")]
        [DataMember(Name = "isValid")]
        public bool IsValid { get; set; }
        /// <summary>
        ///     Whether the form successfully passed through the spam filter.
        /// </summary>
        [JsonPropertyName("passedSpamFilter")]
        [DataMember(Name = "passedSpamFilter")]
        public bool PassedSpamFilter { get; set; }
        /// <summary>
        ///     Spam filtered challenge validation error. Is not used to determine if a form has passed through the spam filter.
        /// </summary>
        [JsonPropertyName("validationError")]
        [DataMember(Name = "validationError")]
        public AltchaSpamFilteredValidationError ValidationError { get; set; }
    }
}
