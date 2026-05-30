using System;
using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.Common.Utilities;
using Ixnas.AltchaNet.Internal.ProofOfWork.Common;
using Ixnas.AltchaNet.Internal.ProofOfWork.Generation;

namespace Ixnas.AltchaNet.Internal.ProofOfWork
{
    internal class ChallengeGenerator
    {
        private readonly string _algorithm;
        private readonly AltchaSha256Configuration _configuration;
        private readonly ChallengeStringGenerator _challengeStringGenerator;
        private readonly RandomNumberGenerator _randomNumberGenerator;
        private readonly SaltGenerator _saltGenerator;
        private readonly SignatureGenerator _signatureGenerator;

        public ChallengeGenerator(ChallengeStringGenerator challengeStringGenerator,
                                  CryptoAlgorithm cryptoAlgorithm,
                                  SaltGenerator saltGenerator,
                                  RandomNumberGenerator randomNumberGenerator,
                                  SignatureGenerator signatureGenerator,
                                  AltchaSha256Configuration configuration)
        {
            _algorithm = cryptoAlgorithm.Name;
            _challengeStringGenerator = challengeStringGenerator;
            _saltGenerator = saltGenerator;
            _randomNumberGenerator = randomNumberGenerator;
            _signatureGenerator = signatureGenerator;
            _configuration = configuration;
        }

        public AltchaChallenge Generate(
            Func<AltchaSha256Configuration, AltchaSha256Configuration> configurationOverrides)
        {
            Guard.NotNull(configurationOverrides);
            var configuration = GetConfigurationFromOverrideFn(configurationOverrides);
            Guard.NotNull(configuration);

            var salt = _saltGenerator.Generate(configuration.Expiry);
            var secretNumber = _randomNumberGenerator.Generate(configuration.Complexity.Counter);
            var maxNumber = _randomNumberGenerator.Max;
            var challenge = _challengeStringGenerator.Generate(salt.Raw, secretNumber);
            var signature = _signatureGenerator.Generate(challenge, configuration.Key)
                                               .ToHexString();
            return new AltchaChallenge
            {
                Algorithm = _algorithm,
                Challenge = challenge,
                Maxnumber = maxNumber,
                Salt = salt.Raw,
                Signature = signature
            };
        }

        private AltchaSha256Configuration GetConfigurationFromOverrideFn(
            Func<AltchaSha256Configuration, AltchaSha256Configuration> configurationOverrides)
        {
#if NET8_0_OR_GREATER
            return configurationOverrides(_configuration);
#else
            var copy = new AltchaSha256Configuration()
            {
                Complexity = new AltchaDeterministicComplexity()
                {
                    Counter = _configuration.Complexity.Counter,
                    Cost = _configuration.Complexity.Cost,
                },
                Key = _configuration.Key,
                Expiry = _configuration.Expiry,
                StoreFactory = _configuration.StoreFactory,
            };
            return configurationOverrides(copy);
#endif
        }
    }
}
