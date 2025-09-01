using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     A result object for solved ALTCHAs.
    /// </summary>
    [DataContract]
    public sealed class AltchaSolverResult
    {
        /// <summary>
        ///     Specifies whether the solver was able to solve the challenge.
        /// </summary>
        [JsonPropertyName("success")]
        [DataMember(Name = "success")]
        public bool Success { get; set; }
        /// <summary>
        ///     If it succeeded, this contains the payload to use for a validation endpoint (usually as a form field named
        ///     "altcha").
        /// </summary>
        [JsonPropertyName("altcha")]
        [DataMember(Name = "altcha")]
        public string Altcha { get; set; }
        /// <summary>
        ///     Solver error.
        /// </summary>
        [JsonPropertyName("error")]
        [DataMember(Name = "error")]
        public AltchaSolverError Error { get; set; }
    }
}
