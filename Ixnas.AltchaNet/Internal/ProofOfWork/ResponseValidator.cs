using System;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Internal.Common.Utilities;
using Ixnas.AltchaNet.Internal.ProofOfWork.Validation;

namespace Ixnas.AltchaNet.Internal.ProofOfWork
{
    internal class ResponseValidator
    {
        private readonly AltchaResponseParser _altchaResponseParser;
        private readonly Func<IAltchaChallengeStore> _storeFactory;

        public ResponseValidator(Func<IAltchaChallengeStore> storeFactory,
                                 AltchaResponseParser altchaResponseParser)
        {
            _storeFactory = storeFactory;
            _altchaResponseParser = altchaResponseParser;
        }

        public async Task<AltchaValidationResult> Validate(string altchaBase64)
        {
            Guard.NotNullOrWhitespace(altchaBase64);

            var store = _storeFactory();
            if (store == null)
                throw new MissingStoreException();

            var isValid = _altchaResponseParser.TryParse(altchaBase64, out var altcha)
                          && !await store.Exists(altcha.Challenge)
                          && altcha.IsValid();

            if (!isValid)
                return new AltchaValidationResult();

            await store.Store(altcha.Challenge, altcha.ExpiryUtc);
            return new AltchaValidationResult { IsValid = true };
        }
    }
}
