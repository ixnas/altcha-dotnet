using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Altcha.Net.Internal.Cryptography
{
    internal class Sha256Crypto : Crypto
    {
        protected override string AlgorithmName => "SHA-256";
        private readonly byte[] _key;

        public Sha256Crypto(byte[] key)
        {
            _key = key;
        }

        public override CryptoChallenge GetCryptoChallenge(string salt, int secretNumber)
        {
            var challenge = GetChallenge(salt, secretNumber);
            var signature = GetSignature(challenge);
            var challengeBase64 = GetChallengeBase64(challenge);

            return new CryptoChallenge
            {
                Challenge = challengeBase64,
                Signature = signature
            };
        }

        public override bool SignatureIsValid(string signature, string challenge)
        {
            using (var sha = new HMACSHA256(_key))
            {
                var signatureBytes = StringToByteArray(signature);
                var challengeBytes = StringToByteArray(challenge);
                var calculatedSignature = sha.ComputeHash(challengeBytes);
                return signatureBytes.SequenceEqual(calculatedSignature);
            }
        }

        public override bool ChallengeIsValid(string challenge, string salt, int secretNumber)
        {
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            var secretNumberBytes = Encoding.UTF8.GetBytes(secretNumber.ToString());
            var concat = saltBytes.Concat(secretNumberBytes)
                                  .ToArray();

            using (var sha = new SHA256Managed())
            {
                var hash = sha.ComputeHash(concat);
                var hashString = BitConverter.ToString(hash)
                                             .Replace("-", string.Empty)
                                             .ToLower();
                return challenge == hashString;
            }
        }

        private static byte[] GetChallenge(string salt, int secretNumber)
        {
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            var secretNumberBytes = Encoding.UTF8.GetBytes(secretNumber.ToString());
            var concat = saltBytes.Concat(secretNumberBytes)
                                  .ToArray();

            using (var sha = new SHA256Managed())
            {
                return sha.ComputeHash(concat);
            }
        }

        private string GetSignature(byte[] challenge)
        {
            using (var sha = new HMACSHA256(_key))
            {
                var signature = sha.ComputeHash(challenge);
                return BitConverter.ToString(signature)
                                   .Replace("-", string.Empty)
                                   .ToLower();
            }
        }

        private static string GetChallengeBase64(byte[] challenge)
        {
            return BitConverter.ToString(challenge)
                               .Replace("-", string.Empty)
                               .ToLower();
        }

        private static byte[] StringToByteArray(string str)
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
    }
}
