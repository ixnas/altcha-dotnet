using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Altcha.Net.Tests;

internal class AltchaFrontEndSimulation
{
    internal class AltchaFrontEndSimulationResult
    {
        public bool Succeeded { get; init; }
        public string AltchaJson { get; init; }
        public int Number { get; init; }
    }

    private class Req
    {
        public string Challenge { get; init; }
        public int Number { get; init; }
        public string Salt { get; init; }
        public string Signature { get; init; }
        public string Algorithm { get; init; }
    }

    public AltchaFrontEndSimulationResult Run(IAltchaChallenge altchaChallenge)
    {
        using var sha = new SHA256Managed();

        for (var number = 0; number <= altchaChallenge.Maxnumber; number++)
        {
            var challenge = altchaChallenge.Salt + $"{number}";
            var challengeBytes = Encoding.UTF8.GetBytes(challenge);

            var attemptedHash = sha.ComputeHash(challengeBytes);
            var validHash = StringToBytes(altchaChallenge.Challenge);

            var succeeded = attemptedHash.SequenceEqual(validHash);
            if (!succeeded)
                continue;

            var altchaJson = GetAtchaJsonBase64(altchaChallenge.Challenge,
                                                number,
                                                altchaChallenge.Salt,
                                                altchaChallenge.Signature);
            return new AltchaFrontEndSimulationResult
            {
                Succeeded = true,
                AltchaJson = altchaJson,
                Number = number
            };
        }

        return new AltchaFrontEndSimulationResult
        {
            Succeeded = false,
            AltchaJson = string.Empty
        };
    }

    private static byte[] StringToBytes(string str)
    {
        var bytes = new List<byte>();
        var uppercase = str.ToLower();
        for (var i = 0; i < uppercase.Length; i += 2)
        {
            var @string = uppercase.Substring(i, 2);
            var @byte = Convert.ToByte(@string, 16);
            bytes.Add(@byte);
        }

        return bytes.ToArray();
    }

    private static string GetAtchaJsonBase64(string challenge,
                                             int number,
                                             string salt,
                                             string signature)
    {
        var req = new Req
        {
            Challenge = challenge,
            Number = number,
            Salt = salt,
            Signature = signature,
            Algorithm = "SHA-256"
        };
        var json = JsonSerializer.SerializeToUtf8Bytes(req,
                                                       new JsonSerializerOptions
                                                       {
                                                           PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                                       });
        return Convert.ToBase64String(json);
    }
}
