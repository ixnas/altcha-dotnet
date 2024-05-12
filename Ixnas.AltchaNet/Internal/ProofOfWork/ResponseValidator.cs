using System.Threading.Tasks;
using Ixnas.AltchaNet.Internal.Common.Utilities;
using Ixnas.AltchaNet.Internal.ProofOfWork.Validation;

namespace Ixnas.AltchaNet.Internal.ProofOfWork
{
    internal class ResponseValidator
    {
        private readonly AltchaResponseParser _altchaResponseParser;
        private readonly IAltchaChallengeStore _store;

        public ResponseValidator(IAltchaChallengeStore store,
                                 AltchaResponseParser altchaResponseParser)
        {
            _store = store;
            _altchaResponseParser = altchaResponseParser;
        }

        public async Task<AltchaValidationResult> Validate(string altchaBase64)
        {
            Guard.NotNullOrWhitespace(altchaBase64);

            var isValid = _altchaResponseParser.TryParse(altchaBase64, out var altcha)
                          && !await _store.Exists(altcha.Challenge)
                          && altcha.IsValid();

            if (!isValid)
                return new AltchaValidationResult();

            await _store.Store(altcha.Challenge, altcha.ExpiryUtc);
            return new AltchaValidationResult { IsValid = true };
        }
    }
}
