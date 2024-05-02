using System.Security.Cryptography;

namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal class Sha256CryptoAlgorithm : CryptoAlgorithm
    {
        private readonly byte[] _key;

        public Sha256CryptoAlgorithm(byte[] key)
        {
            _key = key;
        }

        public string Name => "SHA-256";

        public byte[] GetHash(byte[] bytes)
        {
            using (var sha = new SHA256Managed())
            {
                return sha.ComputeHash(bytes);
            }
        }

        public byte[] GetSignature(byte[] bytes)
        {
            using (var sha = new HMACSHA256(_key))
            {
                return sha.ComputeHash(bytes);
            }
        }
    }
}
