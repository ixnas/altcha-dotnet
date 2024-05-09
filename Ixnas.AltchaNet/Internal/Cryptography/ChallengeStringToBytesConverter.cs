namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal interface ChallengeStringToBytesConverter
    {
        Result<byte[]> Generate(string challenge);
    }
}
