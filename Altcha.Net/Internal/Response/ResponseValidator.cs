using System.Threading.Tasks;
using Altcha.Net.Internal.Cryptography;
using Altcha.Net.Internal.Serialization;

namespace Altcha.Net.Internal.Response
{
    internal class ResponseValidator : IResponseValidator
    {
        private readonly ICryptoValidator _cryptoValidator;
        private readonly IJsonSerializer _serializer;
        private readonly IAltchaChallengeStore _store;

        public ResponseValidator(IAltchaChallengeStore store, ICryptoValidator cryptoValidator, IJsonSerializer serializer)
        {
            _store = store;
            _cryptoValidator = cryptoValidator;
            _serializer = serializer;
        }

        public async Task<IAltchaValidationResult> Validate(string altchaBase64)
        {
            var altcha = _serializer.FromBase64Json<Response>(altchaBase64);

            if (!await IsValid(altcha))
                return new ValidationResult();

            await _store.Store(altcha.Challenge);
            return new ValidationResult { IsValid = true };
        }

        private async Task<bool> IsValid(Response altcha)
        {
            return
                await IsNewChallenge(altcha)
                && _cryptoValidator.AlgorithmMatches(altcha.Algorithm)
                && _cryptoValidator.ChallengeIsValid(altcha.Challenge, altcha.Salt, altcha.Number)
                && _cryptoValidator.SignatureIsValid(altcha.Signature, altcha.Challenge);
        }

        private async Task<bool> IsNewChallenge(Response altcha)
        {
            return !await _store.Exists(altcha.Challenge);
        }
    }
}
