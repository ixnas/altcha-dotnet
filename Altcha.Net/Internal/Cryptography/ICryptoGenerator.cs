namespace Altcha.Net.Internal.Cryptography
{
    internal interface ICryptoGenerator
    {
        CryptoChallenge GetCryptoChallenge(string salt, int secretNumber);
    }
}
