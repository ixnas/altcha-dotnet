namespace Altcha.Net.Internal.Cryptography
{
    internal abstract class Crypto : ICryptoGenerator, ICryptoValidator
    {
        protected abstract string AlgorithmName { get; }
        public abstract CryptoChallenge GetCryptoChallenge(string salt, int secretNumber);
        public abstract bool SignatureIsValid(string signature, string challenge);
        public abstract bool ChallengeIsValid(string challenge, string salt, int secretNumber);

        public bool AlgorithmMatches(string algorithm)
        {
            return algorithm == AlgorithmName;
        }
    }
}
