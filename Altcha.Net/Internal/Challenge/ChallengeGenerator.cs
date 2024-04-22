using Altcha.Net.Internal.Cryptography;

namespace Altcha.Net.Internal.Challenge
{
    internal class ChallengeGenerator : IChallengeGenerator
    {
        private readonly ICryptoGenerator _cryptoGenerator;
        private readonly int _max;
        private readonly int _min;
        private readonly IRandomNumberGenerator _randomNumberGenerator;
        private readonly ISaltGenerator _saltGenerator;

        public ChallengeGenerator(ISaltGenerator saltGenerator,
                                  IRandomNumberGenerator randomNumberGenerator,
                                  ICryptoGenerator cryptoGenerator,
                                  int min,
                                  int max)
        {
            _saltGenerator = saltGenerator;
            _randomNumberGenerator = randomNumberGenerator;
            _cryptoGenerator = cryptoGenerator;
            _min = min;
            _max = max;
        }

        public IAltchaChallenge Generate()
        {
            var salt = _saltGenerator.Generate();
            var secretNumber = _randomNumberGenerator.Generate(_min, _max);
            var challenge = _cryptoGenerator.GetCryptoChallenge(salt, secretNumber);

            return new AltchaChallenge
            {
                Salt = salt,
                Challenge = challenge.Challenge,
                Signature = challenge.Signature,
                Maxnumber = _max
            };
        }
    }
}
