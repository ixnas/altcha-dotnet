namespace Altcha.Net.Internal.Cryptography
{
    internal interface ICryptoValidator
    {
        bool SignatureIsValid(string signature, string challenge);
        bool ChallengeIsValid(string challenge, string salt, int secretNumber);
        bool AlgorithmMatches(string algorithm);
    }
}
