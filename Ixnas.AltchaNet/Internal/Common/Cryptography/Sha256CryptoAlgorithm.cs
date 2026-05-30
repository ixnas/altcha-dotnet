using System.Security.Cryptography;

namespace Ixnas.AltchaNet.Internal.Common.Cryptography
{
    internal class Sha256CryptoAlgorithm : CryptoAlgorithm
    {
        public string Name => "SHA-256";

        public byte[] Hash(byte[] bytes)
        {
#if NET8_0_OR_GREATER
            return SHA256.HashData(bytes);
#else
            using (var sha = new SHA256Managed())
            {
                return sha.ComputeHash(bytes);
            }
#endif
        }

        public byte[] Sign(byte[] bytes, AltchaKey key)
        {
            using (var sha = new HMACSHA256(key.Bytes))
            {
                return sha.ComputeHash(bytes);
            }
        }
    }
}
