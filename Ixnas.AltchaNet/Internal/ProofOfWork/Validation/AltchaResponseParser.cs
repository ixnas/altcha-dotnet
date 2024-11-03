using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.ProofOfWork.Common;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Validation
{
    internal class AltchaResponseParser
    {
        private readonly ChallengeParser _challengeParser;
        private readonly JsonSerializer _serializer;
        private readonly SignatureParser _signatureParser;

        public AltchaResponseParser(JsonSerializer serializer,
                                    ChallengeParser challengeParser,
                                    SignatureParser signatureParser)
        {
            _serializer = serializer;
            _challengeParser = challengeParser;
            _signatureParser = signatureParser;
        }

        public Result<AltchaResponse> Parse(string altchaBase64)
        {
            var altchaParsedResult = _serializer.FromBase64Json<SerializedAltchaResponse>(altchaBase64);
            if (!altchaParsedResult.Success)
                return Result<AltchaResponse>.Fail(altchaParsedResult);

            var deserialized = altchaParsedResult.Value;
            var parseSignatureResult = _signatureParser.Parse(deserialized.Signature);
            if (!parseSignatureResult.Success)
                return Result<AltchaResponse>.Fail(parseSignatureResult);

            var signature = parseSignatureResult.Value;
            var secretNumber = deserialized.Number;
            var challenge = deserialized.Challenge;
            var algorithm = deserialized.Algorithm;
            var salt = deserialized.Salt;

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
