using System;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.Common.Salt;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.Common.Utilities;
using Ixnas.AltchaNet.Internal.ProofOfWork;
using Ixnas.AltchaNet.Internal.ProofOfWork.Common;
using Ixnas.AltchaNet.Internal.ProofOfWork.Generation;
using Ixnas.AltchaNet.Internal.ProofOfWork.Validation;

namespace Ixnas.AltchaNet.Internal
{
    /// <summary>
    ///     Builds the service that provides self-hosted ALTCHA challenges
    /// </summary>
    internal sealed class AltchaServiceBuilder
    {
        private readonly Clock _clock = new DefaultClock();
        private readonly AltchaDeterministicComplexity _complexity = new()
        {
            Counter = new AltchaComplexityCounterRange(Defaults.ComplexityMin, Defaults.ComplexityMax),
            Cost = 1
        };
        private readonly AltchaExpiry _expiry = AltchaExpiry.FromSeconds(Defaults.ExpiryInSeconds);
        private readonly AltchaKey _key;
        private readonly Func<IAltchaChallengeStore> _storeFactory;

        internal AltchaServiceBuilder()
        {
        }

        private AltchaServiceBuilder(Func<IAltchaChallengeStore> storeFactory,
                                     Clock clock,
                                     AltchaKey key,
                                     AltchaDeterministicComplexity complexity,
                                     AltchaExpiry expiry)
        {
            _storeFactory = storeFactory;
            _clock = clock;
            _key = key;
            _complexity = complexity;
            _expiry = expiry;
        }

        /// <summary>
        ///     Returns a new configured service instance.
        /// </summary>
        public AltchaService Build()
        {
            var serializer = new SystemTextJsonSerializer();
            var secretNumberGenerator = new RandomNumberGenerator();
            var cryptoAlgorithm = new Sha256CryptoAlgorithm();
            var saltGenerator = new SaltGenerator(_clock,
                                                  _expiry);
            var saltParser = new SaltParser(_clock);
            var payloadToBytesConverter =
                new SelfHostedPayloadConverter();
            var signatureGenerator =
                new SignatureGenerator(cryptoAlgorithm, payloadToBytesConverter);
            var challengeStringGenerator =
                new ChallengeStringGenerator(cryptoAlgorithm);
            var challengeFactory = new ChallengeParser(cryptoAlgorithm,
                                                       saltParser,
                                                       challengeStringGenerator);
            var signatureParser =
                new SignatureParser(payloadToBytesConverter, cryptoAlgorithm);
            var altchaParser = new AltchaResponseParser(challengeFactory,
                                                        signatureParser);

            var configuration = new AltchaSha256Configuration
            {
                StoreFactory = _storeFactory,
                Key = _key,
                Complexity = _complexity,
                Expiry = _expiry
            };

            var challengeGenerator =
                new ChallengeGenerator(challengeStringGenerator,
                                       cryptoAlgorithm,
                                       saltGenerator,
                                       secretNumberGenerator,
                                       signatureGenerator,
                                       configuration);

            var responseValidator = new ResponseValidator(_storeFactory,
                                                          altchaParser,
                                                          serializer,
                                                          configuration);

            return new AltchaService(challengeGenerator, responseValidator);
        }

#if DEBUG
        /// <summary>
        ///     DEBUG ONLY: Provide an alternative clock implementation. Used for testing time based logic.
        /// </summary>
        /// <param name="clock">An alternative clock implementation.</param>
        /// <returns>A new instance of the builder with the updated configuration.</returns>
        public AltchaServiceBuilder UseClock(Clock clock)
        {
            return new AltchaServiceBuilder(_storeFactory,
                                            clock,
                                            _key,
                                            _complexity,
                                            _expiry);
        }
#endif

        public AltchaServiceBuilder UseSha256(AltchaSha256Configuration configuration)
        {
            return new AltchaServiceBuilder(configuration.StoreFactory,
                                            _clock,
                                            configuration.Key,
                                            configuration.Complexity,
                                            configuration.Expiry);
        }
    }
}
