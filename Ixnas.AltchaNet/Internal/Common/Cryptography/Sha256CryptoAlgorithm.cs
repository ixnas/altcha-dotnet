using System.Security.Cryptography;

namespace Ixnas.AltchaNet.Internal.Common.Cryptography
{
    internal class Sha256CryptoAlgorithm : CryptoAlgorithm
    {
        private readonly byte[] _key;

        public Sha256CryptoAlgorithm(byte[] key)
        {
            _key = key;
        }

        public string Name => "SHA-256";

        public byte[] Hash(byte[] bytes)
        {
            using (var sha = new SHA256Managed())
            {
                return sha.ComputeHash(bytes);
            }
        }

        public byte[] Sign(byte[] bytes)
        {
            using (var sha = new HMACSHA256(_key))
            {
                return sha.ComputeHash(bytes);
            }
        }
    }
}