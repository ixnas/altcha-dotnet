namespace Altcha.Net
{
    public interface IAltchaChallenge
    {
        string Algorithm { get; }
        string Challenge { get; }
        string Salt { get; }
        string Signature { get; }
        int Maxnumber { get; }
    }
}
