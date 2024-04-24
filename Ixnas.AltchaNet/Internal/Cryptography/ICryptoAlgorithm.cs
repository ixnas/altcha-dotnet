namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal interface ICryptoAlgorithm
    {
        string Name { get; }
        byte[] GetHash(byte[] bytes);
        byte[] GetSignature(byte[] bytes);
    }
}
