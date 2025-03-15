using Ixnas.AltchaNet.Internal.Common.Cryptography;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Validation
{
    internal class AltchaResponseParser
    {
        private readonly ChallengeParser _challengeParser;
        private readonly SignatureParser _signatureParser;

        public AltchaResponseParser(ChallengeParser challengeParser,
                                    SignatureParser signatureParser)
        {
            _challengeParser = challengeParser;
            _signatureParser = signatureParser;
        }

        public Result<AltchaResponse> Parse(AltchaNet.AltchaResponse altchaResponse)
        {
            var parseSignatureResult = _signatureParser.Parse(altchaResponse.Signature);
            if (!parseSignatureResult.Success)
                return Result<AltchaResponse>.Fail(parseSignatureResult);

            var signature = parseSignatureResult.Value;
            var secretNumber = altchaResponse.Number;
            var challenge = altchaResponse.Challenge;
            var algorithm = altchaResponse.Algorithm;
            var salt = altchaResponse.Salt;

            var calculatedChallengeResult = _challengeParser.Create(salt, secretNumber);
            if (!calculatedChallengeResult.Success)
                return Result<AltchaResponse>.Fail(calculatedChallengeResult);

            var calculatedChallenge = calculatedChallengeResult.Value;

            return Result<AltchaResponse>.Ok(new AltchaResponse(signature,
                                                                challenge,
                                                                algorithm,
                                                                calculatedChallenge));
        }
    }
}
