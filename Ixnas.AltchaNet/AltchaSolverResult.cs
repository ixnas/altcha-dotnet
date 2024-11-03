namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     A result object for solved ALTCHAs.
    /// </summary>
    public sealed class AltchaSolverResult
    {
        /// <summary>
        ///     Specifies whether the solver was able to solve the challenge.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        ///     If it succeeded, this contains the payload to use for a validation endpoint (usually as a form field named
        ///     "altcha").
        /// </summary>
        public string Altcha { get; set; }
        /// <summary>
        ///     Solver error.
        /// </summary>
        public AltchaSolverError Error { get; set; }
    }
}
