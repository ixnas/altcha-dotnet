namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Entrypoint to the Altcha.Net library.
    /// </summary>
    public static class Altcha
    {
        /// <summary>
        ///     Creates a service builder.
        /// </summary>
        /// <returns>A new service builder instance.</returns>
        public static AltchaServiceBuilder CreateServiceBuilder()
        {
            return new AltchaServiceBuilder();
        }
    }
}
