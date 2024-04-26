namespace Ixnas.AltchaNet.Internal.Salt
{
    internal interface ITimestampedSaltParser
    {
        ITimestampedSalt FromBase64Json(string salt);
    }
}
