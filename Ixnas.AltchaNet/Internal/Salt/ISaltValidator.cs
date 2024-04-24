namespace Ixnas.AltchaNet.Internal.Salt
{
    internal interface ISaltValidator
    {
        bool Validate(string salt);
    }
}
