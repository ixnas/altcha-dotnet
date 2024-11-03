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
            Guard.NotNull<MissingStoreException>(store);

            var validationResult = await IsValidResponse(altchaBase64, store);

            if (!validationResult.Success)
                return validationResult.Error.ToValidationResult();

            var altcha = validationResult.Value;
            await store.Store(altcha.Challenge, altcha.ExpiryUtc);

            return Error.Create(ErrorCode.NoError)
                        .ToValidationResult();
        }

        private async Task<Result<AltchaResponse>> IsValidResponse(
            string altchaBase64,
            IAltchaChallengeStore store)
        {
            var parseResult = _altchaResponseParser.Parse(altchaBase64);
            if (!parseResult.Success)
                return Result<AltchaResponse>.Fail(parseResult);

            var altcha = parseResult.Value;
            var exists = await store.Exists(altcha.Challenge);
            if (exists)
                return Result<AltchaResponse>.Fail(ErrorCode.PreviouslyVerified);

            var validationResult = altcha.Validate();
            return Result<AltchaResponse>.From(validationResult, altcha);
        }
    }
}
