using System;
using System.Linq;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Converters;
using Ixnas.AltchaNet.Internal.Cryptography;
using Ixnas.AltchaNet.Internal.Salt;
using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Response
{
    internal class ResponseValidator
    {
        private readonly BytesStringConverter _bytesStringConverter;
        private readonly Clock _clock;
        private readonly CryptoAlgorithm _cryptoAlgorithm;
        private readonly TimestampedSaltParser _saltParser;
        private readonly JsonSerializer _serializer;
        private readonly IAltchaChallengeStore _store;

        public ResponseValidator(IAltchaChallengeStore store,
                                 JsonSerializer serializer,
                                 TimestampedSaltParser saltParser,
                                 BytesStringConverter bytesStringConverter,
                                 CryptoAlgorithm cryptoAlgorithm,
                                 Clock clock)
        {
            _store = store;
            _serializer = serializer;
            _bytesStringConverter = bytesStringConverter;
            _cryptoAlgorithm = cryptoAlgorithm;
            _clock = clock;
            _saltParser = saltParser;
        }

        public async Task<AltchaValidationResult> Validate(string altchaBase64)
        {
            var altcha = _serializer.FromBase64Json<Response>(altchaBase64);
            var timestamp = _saltParser.FromBase64Json(altcha.Salt)
                                       .GetExpiryUtc();

            if (!await IsValid(altcha, timestamp))
                return new AltchaValidationResult();

            await _store.Store(altcha.Challenge, timestamp);
            return new AltchaValidationResult { IsValid = true };
        }

        private async Task<bool> IsValid(Response altcha, DateTimeOffset timestamp)
        {
            return
                await IsNewChallenge(altcha.Challenge)
                && AlgorithmMatches(altcha.Algorithm)
                && ChallengeIsValid(altcha.Challenge, altcha.Salt, altcha.Number)
                && SignatureIsValid(altcha.Signature, altcha.Challenge)
                && TimestampIsValid(timestamp);
        }

        private async Task<bool> IsNewChallenge(string challenge)
        {
            return !await _store.Exists(challenge);
        }

        private bool AlgorithmMatches(string algorithm)
        {
            return algorithm == _cryptoAlgorithm.Name;
        }

        private bool ChallengeIsValid(string challenge, string salt, int secretNumber)
        {
            var concat = string.Concat(salt, secretNumber.ToString());
            var concatBytes = _bytesStringConverter.GetByteArrayFromUtf8String(concat);
            var hash = _cryptoAlgorithm.GetHash(concatBytes);
            var hashString = _bytesStringConverter.GetHexStringFromBytes(hash);
            return challenge == hashString;
        }

        private bool SignatureIsValid(string signature, string challenge)
        {
            var signatureBytesResult = _bytesStringConverter.GetByteArrayFromHexString(signature);
            var challengeBytesResult = _bytesStringConverter.GetByteArrayFromHexString(challenge);

            var couldConvert = signatureBytesResult.Success && challengeBytesResult.Success;
            if (!couldConvert)
                return false;

            var signatureBytes = signatureBytesResult.Value;
            var challengeBytes = challengeBytesResult.Value;

            var calculatedSignature = _cryptoAlgorithm.GetSignature(challengeBytes);
            return signatureBytes.SequenceEqual(calculatedSignature);
        }

        private bool TimestampIsValid(DateTimeOffset timestamp)
        {
            // stryker disable once equality: Impossible to black box test to exactly now.
            return _clock.GetUtcNow() < timestamp;
        }
    }
}
