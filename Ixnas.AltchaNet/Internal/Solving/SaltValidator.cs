namespace Ixnas.AltchaNet.Internal.Solving
{
    internal interface SaltValidator
    {
        bool IsValid(string salt);
    }
}
