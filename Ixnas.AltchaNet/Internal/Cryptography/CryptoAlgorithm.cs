namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal interface CryptoAlgorithm
    {
        string Name { get; }
        byte[] GetHash(byte[] bytes);
        byte[] GetSignature(byte[] bytes);
    }
}
