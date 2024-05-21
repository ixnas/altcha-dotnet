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

        public bool TryParse(string altchaBase64, out AltchaResponse altcha)
        {
            var parsed = Parse(altchaBase64);
            if (!parsed.Success)
            {
                altcha = null;
                return false;
            }

            altcha = parsed.Value;
            return true;
        }

        private Result<AltchaResponse> Parse(string altchaBase64)
        {
            var altchaParsedResult = _serializer.FromBase64Json<SerializedAltchaResponse>(altchaBase64);
            if (!altchaParsedResult.Success)
                return new Result<AltchaResponse>();

            var deserialized = altchaParsedResult.Value;
            if (!_signatureParser.TryParse(deserialized.Signature, out var signature))
                return new Result<AltchaResponse>();

            var secretNumber = deserialized.Number;
            var challenge = deserialized.Challenge;
            var algorithm = deserialized.Algorithm;
            var salt = deserialized.Salt;

            var calculatedChallengeResult = _challengeParser.Create(salt, secretNumber);
            if (!calculatedChallengeResult.Success)
                return new Result<AltchaResponse>();

            var calculatedChallenge = calculatedChallengeResult.Value;

            return new Result<AltchaResponse>
            {
                Success = true,
                Value = new AltchaResponse(signature,
                                           challenge,
                                           algorithm,
                                           calculatedChallenge)
            };
        }
    }
}
