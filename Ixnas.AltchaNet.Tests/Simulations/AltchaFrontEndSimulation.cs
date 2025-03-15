using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ixnas.AltchaNet.Tests.Abstractions;

namespace Ixnas.AltchaNet.Tests.Simulations
{
    internal class AltchaFrontEndSimulation
    {
        internal class AltchaFrontEndSimulationResult
        {
            public bool Succeeded { get; set; }
            public AltchaResponseSet Altcha { get; set; }
            public int Number { get; set; }
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

                    var altchaResponse = GetAtchaResponse(altchaChallenge.Challenge,
                                                          number,
                                                          altchaChallenge.Salt,
                                                          altchaChallenge.Signature,
                                                          malformSignatureFn,
                                                          malformChallengeFn,
                                                          malformSaltFn,
                                                          replaceSecretNumberFn,
                                                          replaceAlgorithmFn);
                    var altchaJson =
                        JsonSerializer.SerializeToUtf8Bytes(altchaResponse, TestUtils.JsonSerializerOptions);
                    var altchaJson64 = Convert.ToBase64String(altchaJson);
                    return new AltchaFrontEndSimulationResult
                    {
                        Succeeded = true,
                        Altcha = new AltchaResponseSet
                        {
                            Base64 = altchaJson64,
                            Object = altchaResponse
                        },
                        Number = number
                    };
                }

                return new AltchaFrontEndSimulationResult
                {
                    Succeeded = false
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

        private static AltchaResponse GetAtchaResponse(string challenge,
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

            return new AltchaResponse
            {
                Challenge = challenge,
                Number = number,
                Salt = salt,
                Signature = signature,
                Algorithm = algorithm
            };
        }
    }
}
