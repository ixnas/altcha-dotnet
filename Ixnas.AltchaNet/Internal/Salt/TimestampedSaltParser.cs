namespace Ixnas.AltchaNet.Internal.Salt
{
    internal interface TimestampedSaltParser
    {
        Result<TimestampedSalt> Parse(string salt);
    }
}
