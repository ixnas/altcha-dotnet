using System.Collections.Generic;
using System.Linq;
using Ixnas.AltchaNet.Internal;
using Ixnas.AltchaNet.Internal.Common.Converters;
using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.Common.Utilities;
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

            if (string.IsNullOrWhiteSpace(challenge.Challenge))
                return Error.Create(ErrorCode.ChallengeIsInvalidHexString)
                            .ToSolverResult();

            if (string.IsNullOrWhiteSpace(challenge.Signature))
                return Error.Create(ErrorCode.SignatureIsEmpty)
                            .ToSolverResult();

            if (string.IsNullOrWhiteSpace(challenge.Salt))
                return Error.Create(ErrorCode.SaltIsEmpty)
                            .ToSolverResult();

            if (challenge.Maxnumber < 1)
                return Error.Create(ErrorCode.InvalidMaxNumber)
                            .ToSolverResult();

            if (string.IsNullOrWhiteSpace(challenge.Algorithm)
                || challenge.Algorithm != _cryptoAlgorithm.Name)
                return Error.Create(ErrorCode.AlgorithmNotSupported)
                            .ToSolverResult();

            if (!_saltValidator.IsValid(challenge.Salt))
                return Error.Create(ErrorCode.ChallengeExpired)
                            .ToSolverResult();

            var targetHashResult = GetTargetHash(challenge.Challenge);
            if (!targetHashResult.Success)
                return Error.Create(ErrorCode.ChallengeIsInvalidHexString)
                            .ToSolverResult();

            var targetHash = targetHashResult.Value;

            var secretNumberResult =
                SolveSecretNumber(challenge.Salt, challenge.Maxnumber, targetHash);
            if (!secretNumberResult.Success)
                return Error.Create(ErrorCode.CouldNotSolveChallenge)
                            .ToSolverResult();
            var secretNumber = secretNumberResult.Value;

            var altcha = GenerateAltchaResponse(challenge, secretNumber);
            return Error.Create(ErrorCode.NoError)
                        .ToSolverResult(altcha);
        }

        private Result<int> SolveSecretNumber(string salt, int maxNumber, byte[] targetHash)
        {
            for (var number = 0; number <= maxNumber; number++)
            {
                var attemptedHash = GenerateAttemptedHash(salt, number);
                var succeeded = HashesMatch(attemptedHash, targetHash);
                if (!succeeded)
                    continue;

                return Result<int>.Ok(number);
            }

            return Result<int>.Fail();
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
            var response = new AltchaResponse
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
