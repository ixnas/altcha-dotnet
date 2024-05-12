namespace Ixnas.AltchaNet.Internal.Common.Cryptography
{
    internal interface CryptoAlgorithm
    {
        string Name { get; }
        byte[] Hash(byte[] bytes);
        byte[] Sign(byte[] bytes);
    }
}
