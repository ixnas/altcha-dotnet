using System;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.Common.Salt;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.Common.Utilities;
using Ixnas.AltchaNet.Internal.Solving;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Builds an ALTCHA solver instance.
    /// </summary>
    [Obsolete("Will be removed in the next major version. Please use Altcha.CreateSolver() instead.")]
    public sealed class AltchaSolverBuilder
    {
        private readonly Clock _clock = new DefaultClock();
        private readonly bool _ignoreExpiry;

        internal AltchaSolverBuilder()
        {
        }

        private AltchaSolverBuilder(Clock clock, bool ignoreExpiry)
        {
            _clock = clock;
            _ignoreExpiry = ignoreExpiry;
        }

        /// <summary>
        ///     (Optional) Disables checking for expiry before attempting to solve a challenge.
        /// </summary>
        /// <returns>A new instance of the builder with the updated configuration.</returns>
        public AltchaSolverBuilder IgnoreExpiry()
        {
            return new AltchaSolverBuilder(_clock, true);
        }

        /// <summary>
        ///     Returns a new configured solver instance.
        /// </summary>
        public AltchaSolver Build()
        {
            var serializer = new SystemTextJsonSerializer();
            var cryptoAlgorithm = new Sha256CryptoAlgorithm();
            var saltValidator = GetSaltValidator();
            return new AltchaSolver(cryptoAlgorithm,
                                    serializer,
                                    saltValidator);
        }

        private SaltValidator GetSaltValidator()
        {
            var saltParser = new SaltParser(_clock);
            if (_ignoreExpiry)
                return new IgnoreSaltValidator();
            return new ExpirySaltValidator(saltParser);
        }
    }
}
