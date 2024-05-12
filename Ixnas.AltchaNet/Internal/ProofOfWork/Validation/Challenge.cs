using System;
using Ixnas.AltchaNet.Internal.Common.Salt;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Validation
{
    internal class Challenge
    {
        public DateTimeOffset ExpiryUtc => _salt.ExpiryUtc;
        private readonly string _algorithm;
        private readonly string _challengeString;
        private readonly Salt _salt;

        public Challenge(string algorithm,
                               Salt salt,
                               string challengeString)
        {
            _algorithm = algorithm;
            _salt = salt;
            _challengeString = challengeString;
        }

        public bool MatchesChallengeString(string challengeString)
        {
            return challengeString == _challengeString;
        }

        public bool MatchesAlgorithm(string algorithm)
        {
            return algorithm == _algorithm;
        }

        public bool HasExpired()
        {
            return _salt.HasExpired();
        }
    }
}
