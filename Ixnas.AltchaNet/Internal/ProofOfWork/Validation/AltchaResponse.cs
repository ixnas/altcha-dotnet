using System;
using Ixnas.AltchaNet.Internal.Common.Cryptography;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Validation
{
    internal class AltchaResponse
    {
        public string Challenge { get; }
        public DateTimeOffset ExpiryUtc => _challenge.ExpiryUtc;
        private readonly string _algorithm;
        private readonly Challenge _challenge;
        private readonly Signature _signature;

        public AltchaResponse(Signature signature,
                              string challengeString,
                              string algorithm,
                              Challenge challenge)
        {
            _signature = signature;
            Challenge = challengeString;
            _algorithm = algorithm;
            _challenge = challenge;
        }

        public Result Validate()
        {
            if (!_challenge.MatchesAlgorithm(_algorithm))
                return Result.Fail(ErrorCode.AlgorithmDoesNotMatch);
            if (!_challenge.MatchesChallengeString(Challenge))
                return Result.Fail(ErrorCode.ChallengeDoesNotMatch);
            if (!_signature.PayloadIsValid(Challenge)
                           .Success)
                return Result.Fail(ErrorCode.PayloadDoesNotMatchSignature);
            if (_challenge.HasExpired())
                return Result.Fail(ErrorCode.ChallengeExpired);

            return Result.Ok();
        }
    }
}
