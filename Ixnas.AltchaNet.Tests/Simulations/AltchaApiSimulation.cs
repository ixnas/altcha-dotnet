using System.Security.Cryptography;
using System.Text;

namespace Ixnas.AltchaNet.Tests.Simulations;

internal class AltchaApiSimulation
{
    private readonly string _apiSecret;

    public AltchaApiSimulation(string apiSecret)
    {
        _apiSecret = apiSecret;
    }

    public AltchaChallenge Generate(int expiryOffsetSeconds = 0)
    {
        var nowSeconds = DateTimeOffset.UtcNow.AddSeconds(expiryOffsetSeconds)
                                       .ToUnixTimeSeconds();
        var randomString = "b9f517664af74946e13c75a5";
        var salt = $"{nowSeconds}.{randomString}";
        var randomNumber = 2;
        var key = Encoding.UTF8.GetBytes(_apiSecret);
        using var hmac = new HMACSHA256(key);
        using var sha = SHA256.Create();
        var challengeRaw = Encoding.UTF8.GetBytes($"{salt}{randomNumber}");
        var challengeBytes = sha.ComputeHash(challengeRaw);
        var challenge = BitConverter.ToString(challengeBytes)
                                    .Replace("-", string.Empty)
                                    .ToLower();
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(challenge));
        var signature = BitConverter.ToString(signatureBytes)
                                    .Replace("-", string.Empty)
                                    .ToLower();
        return new AltchaChallenge
        {
            Algorithm = "SHA-256",
            Challenge = challenge,
            Maxnumber = 3,
            Salt = salt,
            Signature = signature
        };
    }
}
