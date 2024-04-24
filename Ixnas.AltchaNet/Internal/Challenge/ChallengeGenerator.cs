using Ixnas.AltchaNet.Internal.Converters;
using Ixnas.AltchaNet.Internal.Cryptography;
using Ixnas.AltchaNet.Internal.Salt;

namespace Ixnas.AltchaNet.Internal.Challenge
{
    internal class ChallengeGenerator : IChallengeGenerator
    {
        private readonly IBytesStringConverter _bytesStringConverter;
        private readonly ICryptoAlgorithm _cryptoAlgorithm;
        private readonly int _max;
        private readonly int _min;
        private readonly IRandomNumberGenerator _randomNumberGenerator;
        private readonly ISaltGenerator _saltGenerator;

        public ChallengeGenerator(ISaltGenerator saltGenerator,
                                  IRandomNumberGenerator randomNumberGenerator,
                                  IBytesStringConverter bytesStringConverter,
                                  ICryptoAlgorithm cryptoAlgorithm,
                                  int min,
                                  int max)
        {
            _saltGenerator = saltGenerator;
            _randomNumberGenerator = randomNumberGenerator;
            _bytesStringConverter = bytesStringConverter;
            _cryptoAlgorithm = cryptoAlgorithm;
            _min = min;
            _max = max;
        }

        public AltchaChallenge Generate()
        {
            var algorithm = _cryptoAlgorithm.Name;
            var salt = _saltGenerator.Generate();
            var secretNumber = _randomNumberGenerator.Generate(_min, _max);
            var challenge = string.Concat(salt, secretNumber.ToString());
            var challengeBytes = _bytesStringConverter.GetByteArrayFromUtf8String(challenge);
            var challengeHash = _cryptoAlgorithm.GetHash(challengeBytes);
            var signature = _cryptoAlgorithm.GetSignature(challengeHash);

            var signatureHex = _bytesStringConverter.GetHexStringFromBytes(signature);
            var challengeHex = _bytesStringConverter.GetHexStringFromBytes(challengeHash);

            return new AltchaChallenge
            {
                Algorithm = algorithm,
                Salt = salt,
                Challenge = challengeHex,
                Signature = signatureHex,
                Maxnumber = _max
            };
        }
    }
}
