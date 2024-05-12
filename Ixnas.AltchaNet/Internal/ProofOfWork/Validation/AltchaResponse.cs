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

        public bool IsValid()
        {
            return _challenge.MatchesAlgorithm(_algorithm)
                   && _challenge.MatchesChallengeString(Challenge)
                   && _signature.PayloadIsValid(Challenge)
                   && _challenge.HasExpired();
        }
    }
}
