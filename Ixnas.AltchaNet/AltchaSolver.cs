using System.Collections.Generic;
using System.Linq;
using Ixnas.AltchaNet.Internal;
using Ixnas.AltchaNet.Internal.Common.Converters;
using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.Common.Utilities;
using Ixnas.AltchaNet.Internal.ProofOfWork.Common;
using Ixnas.AltchaNet.Internal.Solving;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Solves ALTCHA challenges. Can be used for machine-to-machine ALTCHAs.
    /// </summary>
    public sealed class AltchaSolver
    {
        private readonly CryptoAlgorithm _cryptoAlgorithm;
        private readonly SaltValidator _saltValidator;
        private readonly JsonSerializer _serializer;

        internal AltchaSolver(CryptoAlgorithm cryptoAlgorithm,
                              JsonSerializer serializer,
                              SaltValidator saltValidator)
        {
            _cryptoAlgorithm = cryptoAlgorithm;
            _serializer = serializer;
            _saltValidator = saltValidator;
        }

        /// <summary>
        ///     Solves an ALTCHA challenge.
        /// </summary>
        /// <param name="challenge">The ALTCHA challenge to solve.</param>
        /// <returns>
        ///     A result object containing a base64 JSON encoded payload. You can use this as the "altcha" parameter to the
        ///     validation endpoint.
        /// </returns>
        public AltchaSolverResult Solve(AltchaChallenge challenge)
        {
            Guard.NotNull(challenge);

            if (string.IsNullOrWhiteSpace(challenge.Challenge)
                || string.IsNullOrWhiteSpace(challenge.Signature)
                || string.IsNullOrWhiteSpace(challenge.Algorithm)
                || string.IsNullOrWhiteSpace(challenge.Salt)
                || challenge.Maxnumber < 1
                || challenge.Algorithm != _cryptoAlgorithm.Name)
                return new AltchaSolverResult();

            if (!_saltValidator.IsValid(challenge.Salt))
                return new AltchaSolverResult();

            var targetHashResult = GetTargetHash(challenge.Challenge);
            if (!targetHashResult.Success)
                return new AltchaSolverResult();
            var targetHash = targetHashResult.Value;

            var secretNumberResult =
                SolveSecretNumber(challenge.Salt, challenge.Maxnumber, targetHash);
            if (!secretNumberResult.Success)
                return new AltchaSolverResult();
            var secretNumber = secretNumberResult.Value;

            return new AltchaSolverResult
            {
                Success = true,
                Altcha = GenerateAltchaResponse(challenge, secretNumber)
            };
        }

        private Result<int> SolveSecretNumber(string salt, int maxNumber, byte[] targetHash)
        {
            for (var number = 0; number <= maxNumber; number++)
            {
                var attemptedHash = GenerateAttemptedHash(salt, number);
                var succeeded = HashesMatch(attemptedHash, targetHash);
                if (!succeeded)
                    continue;

                return new Result<int>
                {
                    Success = true,
                    Value = number
                };
            }

            return new Result<int>();
        }

        private byte[] GenerateAttemptedHash(string salt, int number)
        {
            var challenge = $"{salt}{number}";
            var challengeBytes = ByteConverter.GetByteArrayFromUtf8String(challenge);
            return _cryptoAlgorithm.Hash(challengeBytes);
        }

        private static Result<byte[]> GetTargetHash(string challengeString)
        {
            return ByteConverter.GetByteArrayFromHexString(challengeString);
        }

        private static bool HashesMatch(IEnumerable<byte> hashA, IEnumerable<byte> hashB)
        {
            return hashA.SequenceEqual(hashB);
        }

        private string GenerateAltchaResponse(AltchaChallenge altchaChallenge, int number)
        {
            var response = new SerializedAltchaResponse
            {
                Challenge = altchaChallenge.Challenge,
                Number = number,
                Salt = altchaChallenge.Salt,
                Signature = altchaChallenge.Signature,
                Algorithm = _cryptoAlgorithm.Name
            };
            return _serializer.ToBase64Json(response);
        }
    }
}
