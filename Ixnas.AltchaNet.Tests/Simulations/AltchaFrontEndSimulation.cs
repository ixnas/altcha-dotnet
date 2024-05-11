using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ixnas.AltchaNet.Tests.Simulations;

internal class AltchaFrontEndSimulation
{
    internal class AltchaFrontEndSimulationResult
    {
        public bool Succeeded { get; init; }
        public string? AltchaJson { get; init; }
        public int Number { get; init; }
    }

    [Serializable]
    private class Req
    {
        public string? Challenge { get; init; }
        public int Number { get; init; }
        public string? Salt { get; init; }
        public string? Signature { get; init; }
        public string? Algorithm { get; init; }
    }

#pragma warning disable CA1822
    public AltchaFrontEndSimulationResult Run(AltchaChallenge altchaChallenge,
#pragma warning restore CA1822
                                              Func<string, string>? malformSignatureFn = null,
                                              Func<string, string>? malformChallengeFn = null,
                                              Func<string, string>? malformSaltFn = null)
    {
        for (var number = 0; number <= altchaChallenge.Maxnumber; number++)
        {
            var challenge = altchaChallenge.Salt + $"{number}";
            var challengeBytes = Encoding.UTF8.GetBytes(challenge);

            var attemptedHash = SHA256.HashData(challengeBytes);
            var validHash = StringToBytes(altchaChallenge.Challenge);

            var succeeded = attemptedHash.SequenceEqual(validHash);
            if (!succeeded)
                continue;

            var altchaJson = GetAtchaJsonBase64(altchaChallenge.Challenge,
                                                number,
                                                altchaChallenge.Salt,
                                                altchaChallenge.Signature,
                                                malformSignatureFn,
                                                malformChallengeFn,
                                                malformSaltFn);
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

    // ReSharper disable once ReturnTypeCanBeEnumerable.Local
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
                                             string signature,
                                             Func<string, string>? malformSignatureFn,
                                             Func<string, string>? malformChallengeFn,
                                             Func<string, string>? malformSaltFn)
    {
        if (malformSignatureFn != null)
            signature = malformSignatureFn(signature);

        if (malformChallengeFn != null)
            challenge = malformChallengeFn(challenge);

        if (malformSaltFn != null)
            salt = malformSaltFn(salt);

        var req = new Req
        {
            Challenge = challenge,
            Number = number,
            Salt = salt,
            Signature = signature,
            Algorithm = "SHA-256"
        };
        var json = JsonSerializer.SerializeToUtf8Bytes(req, TestUtils.JsonSerializerOptions);
        return Convert.ToBase64String(json);
    }
}
