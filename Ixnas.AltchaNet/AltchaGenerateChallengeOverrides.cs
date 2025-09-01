using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Allows overriding configuration options when generating ALTCHA challenges.
    /// </summary>
    [DataContract]
    public sealed class AltchaGenerateChallengeOverrides
    {
        /// <summary>
        ///     Overrides the configured complexity.
        /// </summary>
        [JsonPropertyName("complexity")]
        [DataMember(Name = "complexity")]
        public AltchaComplexity? Complexity { get; set; }
        /// <summary>
        ///     Overrides the configured expiry.
        /// </summary>
        [JsonPropertyName("expiry")]
        [DataMember(Name = "expiry")]
        public AltchaExpiry? Expiry { get; set; }
    }
}
