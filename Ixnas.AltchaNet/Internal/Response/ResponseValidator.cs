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
        [Serializable]
        private class SerializedResponse
        {
            public string Algorithm { get; set; }
            public string Challenge { get; set; }
            public int Number { get; set; }
            public string Salt { get; set; }
            public string Signature { get; set; }
        }

        private readonly BytesStringConverter _bytesStringConverter;
        private readonly ChallengeStringToBytesConverter _challengeStringToBytesConverter;
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
                                 Clock clock,
                                 ChallengeStringToBytesConverter challengeStringToBytesConverter)
        {
            _store = store;
            _serializer = serializer;
            _bytesStringConverter = bytesStringConverter;
            _cryptoAlgorithm = cryptoAlgorithm;
            _clock = clock;
            _challengeStringToBytesConverter = challengeStringToBytesConverter;
            _saltParser = saltParser;
        }

        public async Task<AltchaValidationResult> Validate(string altchaBase64)
        {
            if (string.IsNullOrWhiteSpace(altchaBase64))
                throw new ArgumentNullException();

            var isValid = TryParseAltchaBase64(altchaBase64, out var altcha, out var timestamp)
                          && await IsNewChallenge(altcha.Challenge)
                          && AlgorithmMatches(altcha.Algorithm)
                          && ChallengeIsValid(altcha.Challenge, altcha.Salt, altcha.Number)
                          && SignatureIsValid(altcha.Signature, altcha.Challenge)
                          && TimestampIsValid(timestamp);

            if (!isValid)
                return new AltchaValidationResult();

            await _store.Store(altcha.Challenge, timestamp);
            return new AltchaValidationResult { IsValid = true };
        }

        private bool TryParseAltchaBase64(string altchaBase64,
                                          out SerializedResponse altcha,
                                          out DateTimeOffset timestamp)
        {
            var altchaParsedResult = _serializer.FromBase64Json<SerializedResponse>(altchaBase64);
            if (!altchaParsedResult.Success)
            {
                altcha = null;
                return false;
            }

            altcha = altchaParsedResult.Value;

            var saltParsedResult = _saltParser.Parse(altcha.Salt);
            if (saltParsedResult.Success)
                timestamp = saltParsedResult.Value.GetExpiryUtc();

            return true;
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
            var challengeBytesResult = _challengeStringToBytesConverter.Generate(challenge);

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
