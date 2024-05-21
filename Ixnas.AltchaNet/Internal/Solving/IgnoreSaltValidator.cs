namespace Ixnas.AltchaNet.Internal.Solving
{
    internal class IgnoreSaltValidator : SaltValidator
    {
        public bool IsValid(string salt)
        {
            return true;
        }
    }
}
