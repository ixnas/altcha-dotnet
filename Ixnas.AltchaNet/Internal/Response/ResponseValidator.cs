using System;
using System.Linq;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Internal.Converters;
using Ixnas.AltchaNet.Internal.Cryptography;
using Ixnas.AltchaNet.Internal.Salt;
using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Response
{
    internal class ResponseValidator : IResponseValidator
    {
        private readonly IBytesStringConverter _bytesStringConverter;
        private readonly ICryptoAlgorithm _cryptoAlgorithm;
        private readonly int _expiryInSeconds;
        private readonly IJsonSerializer _serializer;
        private readonly IAltchaChallengeStore _store;

        public ResponseValidator(IAltchaChallengeStore store,
                                 IJsonSerializer serializer,
                                 IBytesStringConverter bytesStringConverter,
                                 ICryptoAlgorithm cryptoAlgorithm,
                                 int expiryInSeconds)
        {
            _store = store;
            _serializer = serializer;
            _bytesStringConverter = bytesStringConverter;
            _cryptoAlgorithm = cryptoAlgorithm;
            _expiryInSeconds = expiryInSeconds;
        }

        public async Task<AltchaValidationResult> Validate(string altchaBase64)
        {
            var altcha = _serializer.FromBase64Json<Response>(altchaBase64);
            var timestamp = GetTimestampFromSalt(altcha.Salt);

            if (!await IsValid(altcha, timestamp))
                return new AltchaValidationResult();

            await _store.Store(altcha.Challenge, timestamp);
            return new AltchaValidationResult { IsValid = true };
        }

        private DateTimeOffset GetTimestampFromSalt(string salt)
        {
            var deserialized =
                _serializer.FromBase64Json<TimestampedSaltGenerator.Salt>(salt);
            return DateTimeOffset.FromUnixTimeMilliseconds(deserialized.T)
                                 .AddSeconds(_expiryInSeconds);
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

        private static bool TimestampIsValid(DateTimeOffset timestamp)
        {
            // stryker disable once equality: Impossible to black box test to exactly now.
            return DateTimeOffset.UtcNow < timestamp;
        }
    }
}
