using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Entrypoint to the Altcha.Net library.
    /// </summary>
    public static class Altcha
    {
        /// <summary>
        ///     Creates a service for self-hosted ALTCHA challenges.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <returns>A new service instance.</returns>
        public static AltchaService CreateService(AltchaSha256Configuration configuration)
        {
            return new AltchaServiceBuilder()
                   .UseSha256(configuration)
                   .Build();
        }

#if DEBUG
        /// <summary>
        ///     Creates a service for self-hosted ALTCHA challenges.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="clock">The clock implementation to use.</param>
        /// <returns>A new service instance.</returns>
        public static AltchaService CreateService(AltchaSha256Configuration configuration, Clock clock)
        {
            return new AltchaServiceBuilder()
                   .UseSha256(configuration)
                   .UseClock(clock)
                   .Build();
        }
#endif

        /// <summary>
        ///     Creates an ALTCHA solver.
        /// </summary>
        /// <returns>A new solver instance.</returns>
        public static AltchaSolver CreateSolver()
        {
            return new AltchaSolverBuilder()
                .Build();
        }

        /// <summary>
        ///     Creates an ALTCHA solver.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <returns>A new solver instance.</returns>
        public static AltchaSolver CreateSolver(AltchaSolverConfiguration configuration)
        {
            if (configuration.IgnoreExpiry)
                return new AltchaSolverBuilder()
                       .IgnoreExpiry()
                       .Build();
            return new AltchaSolverBuilder()
                .Build();
        }
    }
}
