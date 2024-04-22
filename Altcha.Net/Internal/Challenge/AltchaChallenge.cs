namespace Altcha.Net.Internal.Challenge
{
    internal class AltchaChallenge : IAltchaChallenge
    {
        public string Algorithm => "SHA-256";
        public string Challenge { get; internal set; }
        public string Salt { get; internal set; }
        public string Signature { get; internal set; }
        public int Maxnumber { get; internal set; }
    }
}
