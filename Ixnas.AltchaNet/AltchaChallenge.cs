namespace Ixnas.AltchaNet
{
    public sealed class AltchaChallenge
    {
        public string Algorithm { get; set; }
        public string Challenge { get; set; }
        public string Salt { get; set; }
        public string Signature { get; set; }
        public int Maxnumber { get; set; }
    }
}
