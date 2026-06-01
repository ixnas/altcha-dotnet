using System.Security.Cryptography;

namespace Ixnas.AltchaNet.Internal.Common.Cryptography
{
    internal class Sha256CryptoAlgorithm : CryptoAlgorithm
    {
        public string Name => "SHA-256";

        public byte[] Hash(byte[] bytes)
        {
            return SHA256.HashData(bytes);
        }

        public byte[] Sign(byte[] bytes, AltchaKey key)
        {
            using var sha = new HMACSHA256(key.Bytes);
            return sha.ComputeHash(bytes);
        }
    }
}
