using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ixnas.AltchaNet.Tests.Simulations
{
    internal class AltchaFrontEndSimulation
    {
        internal class AltchaFrontEndSimulationResult
        {
            public bool Succeeded { get; set; }
            public string AltchaJson { get; set; }
            public int Number { get; set; }
        }

        [Serializable]
        private class Req
        {
            public string Challenge { get; set; }
            public int Number { get; set; }
            public string Salt { get; set; }
            public string Signature { get; set; }
            public string Algorithm { get; set; }
        }

#pragma warning disable CA1822
        public AltchaFrontEndSimulationResult Run(AltchaChallenge altchaChallenge,
                                                  Func<string, string> malformSignatureFn = null,
                                                  Func<string, string> malformChallengeFn = null,
                                                  Func<string, string> malformSaltFn = null,
                                                  Func<int> replaceSecretNumberFn = null,
                                                  Func<string> replaceAlgorithmFn = null)
#pragma warning restore CA1822
        {
            using (var sha = SHA256.Create())
            {
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
                                                        altchaChallenge.Signature,
                                                        malformSignatureFn,
                                                        malformChallengeFn,
                                                        malformSaltFn,
                                                        replaceSecretNumberFn,
                                                        replaceAlgorithmFn);
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
                                                 Func<string, string> malformSignatureFn,
                                                 Func<string, string> malformChallengeFn,
                                                 Func<string, string> malformSaltFn,
                                                 Func<int> replaceSecretNumberFn,
                                                 Func<string> replaceAlgorithmFn)
        {
            var algorithm = "SHA-256";
            if (malformSignatureFn != null)
                signature = malformSignatureFn(signature);

            if (malformChallengeFn != null)
                challenge = malformChallengeFn(challenge);

            if (malformSaltFn != null)
                salt = malformSaltFn(salt);

            if (replaceSecretNumberFn != null)
                number = replaceSecretNumberFn();

            if (replaceAlgorithmFn != null)
                algorithm = replaceAlgorithmFn();

            var req = new Req
            {
                Challenge = challenge,
                Number = number,
                Salt = salt,
                Signature = signature,
                Algorithm = algorithm
            };
            var json = JsonSerializer.SerializeToUtf8Bytes(req, TestUtils.JsonSerializerOptions);
            return Convert.ToBase64String(json);
        }
    }
}
