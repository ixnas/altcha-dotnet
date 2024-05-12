namespace Ixnas.AltchaNet.Internal.Common.Salt
{
    internal interface SaltParser
    {
        Result<Salt> Parse(string salt);
    }
}
