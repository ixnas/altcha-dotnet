namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     An ALTCHA challenge that can be solved by the client-side widget.
    /// </summary>
    public sealed class AltchaChallenge
    {
        /// <summary>
        ///     The algorithm to use.
        /// </summary>
        public string Algorithm { get; set; }
        /// <summary>
        ///     The hash that the client should match as a hex string.
        /// </summary>
        public string Challenge { get; set; }
        /// <summary>
        ///     The salt that the client should use to solve the challenge.
        /// </summary>
        public string Salt { get; set; }
        /// <summary>
        ///     Ensures the challenge is not tampered with by the client.
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        ///     The highest random number that the challenge could be.
        /// </summary>
        public int Maxnumber { get; set; }
    }
}
