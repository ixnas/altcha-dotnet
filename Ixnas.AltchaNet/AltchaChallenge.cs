using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     An ALTCHA challenge that can be solved by the client-side widget.
    /// </summary>
    [DataContract]
    public sealed class AltchaChallenge
    {
        /// <summary>
        ///     The algorithm to use.
        /// </summary>
        [JsonPropertyName("algorithm")]
        [DataMember(Name = "algorithm")]
        public string Algorithm { get; set; }
        /// <summary>
        ///     The hash that the client should match as a hex string.
        /// </summary>
        [JsonPropertyName("challenge")]
        [DataMember(Name = "challenge")]
        public string Challenge { get; set; }
        /// <summary>
        ///     The salt that the client should use to solve the challenge.
        /// </summary>
        [JsonPropertyName("salt")]
        [DataMember(Name = "salt")]
        public string Salt { get; set; }
        /// <summary>
        ///     Ensures the challenge is not tampered with by the client.
        /// </summary>
        [JsonPropertyName("signature")]
        [DataMember(Name = "signature")]
        public string Signature { get; set; }
        /// <summary>
        ///     The highest random number that the challenge could be.
        /// </summary>
        [JsonPropertyName("maxnumber")]
        [DataMember(Name = "maxnumber")]
        public int Maxnumber { get; set; }
    }
}
