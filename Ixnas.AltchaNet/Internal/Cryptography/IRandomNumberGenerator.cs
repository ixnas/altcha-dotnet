namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal interface IRandomNumberGenerator
    {
        int Generate(int min, int max);
    }
}