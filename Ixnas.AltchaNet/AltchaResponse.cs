using System;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     An ALTCHA response as provided by the client-side widget.
    /// </summary>
    [Serializable]
    public sealed class AltchaResponse
    {
        /// <summary>
        ///     The hash that the client should match as a hex string.
        /// </summary>
        public string Challenge { get; set; }
        /// <summary>
        ///     The number that solved the challenge.
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        ///     The salt that the client should use to solve the challenge.
        /// </summary>
        public string Salt { get; set; }
        /// <summary>
        ///     Ensures the challenge is not tampered with by the client.
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        ///     The algorithm to use.
        /// </summary>
        public string Algorithm { get; set; }
    }
}
