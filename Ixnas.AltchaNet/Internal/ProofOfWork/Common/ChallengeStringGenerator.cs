using Ixnas.AltchaNet.Internal.Common.Converters;
using Ixnas.AltchaNet.Internal.Common.Cryptography;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Common
{
    internal class ChallengeStringGenerator
    {
        private readonly CryptoAlgorithm _cryptoAlgorithm;

        public ChallengeStringGenerator(CryptoAlgorithm cryptoAlgorithm)
        {
            _cryptoAlgorithm = cryptoAlgorithm;
        }

        public string Generate(string salt, int secretNumber)
        {
            var concat = string.Concat(salt, secretNumber.ToString());
            var concatBytes = ByteConverter.GetByteArrayFromUtf8String(concat);
            var hash = _cryptoAlgorithm.Hash(concatBytes);
            var hashString = ByteConverter.GetHexStringFromBytes(hash);
            return hashString;
        }
    }
}
