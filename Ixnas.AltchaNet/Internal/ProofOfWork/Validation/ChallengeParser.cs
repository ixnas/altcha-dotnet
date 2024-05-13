using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.Common.Salt;
using Ixnas.AltchaNet.Internal.ProofOfWork.Common;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Validation
{
    internal class ChallengeParser
    {
        private readonly ChallengeStringGenerator _challengeStringGenerator;
        private readonly CryptoAlgorithm _cryptoAlgorithm;
        private readonly SaltParser _saltParser;

        public ChallengeParser(CryptoAlgorithm cryptoAlgorithm,
                               SaltParser saltParser,
                               ChallengeStringGenerator challengeStringGenerator)
        {
            _cryptoAlgorithm = cryptoAlgorithm;
            _saltParser = saltParser;
            _challengeStringGenerator = challengeStringGenerator;
        }

        public Result<Challenge> Create(string salt, int secretNumber)
        {
            var parsedSaltResult = _saltParser.Parse(salt);
            if (!parsedSaltResult.Success)
                return new Result<Challenge>();
            var parsedSalt = parsedSaltResult.Value;
            var challengeString = _challengeStringGenerator.Generate(salt, secretNumber);
            return new Result<Challenge>
            {
                Success = true,
                Value = new Challenge(_cryptoAlgorithm.Name,
                                      parsedSalt,
                                      challengeString)
            };
        }
    }
}
