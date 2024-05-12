using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.ProofOfWork.Common;
using Ixnas.AltchaNet.Internal.ProofOfWork.Generation;

namespace Ixnas.AltchaNet.Internal.ProofOfWork
{
    internal class ChallengeGenerator
    {
        private readonly string _algorithm;
        private readonly ChallengeStringGenerator _challengeStringGenerator;
        private readonly RandomNumberGenerator _randomNumberGenerator;
        private readonly SaltGenerator _saltGenerator;
        private readonly SignatureGenerator _signatureGenerator;

        public ChallengeGenerator(ChallengeStringGenerator challengeStringGenerator,
                                  CryptoAlgorithm cryptoAlgorithm,
                                  SaltGenerator saltGenerator,
                                  RandomNumberGenerator randomNumberGenerator,
                                  SignatureGenerator signatureGenerator)
        {
            _algorithm = cryptoAlgorithm.Name;
            _challengeStringGenerator = challengeStringGenerator;
            _saltGenerator = saltGenerator;
            _randomNumberGenerator = randomNumberGenerator;
            _signatureGenerator = signatureGenerator;
        }

        public AltchaChallenge Generate()
        {
            var salt = _saltGenerator.Generate();
            var secretNumber = _randomNumberGenerator.Generate();
            var maxNumber = _randomNumberGenerator.Max;
            var challenge = _challengeStringGenerator.Generate(salt.Raw, secretNumber);
            var signature = _signatureGenerator.Generate(challenge)
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
    }
}
