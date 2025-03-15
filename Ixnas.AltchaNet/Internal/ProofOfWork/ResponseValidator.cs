using System;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.Common.Utilities;
using Ixnas.AltchaNet.Internal.ProofOfWork.Validation;

namespace Ixnas.AltchaNet.Internal.ProofOfWork
{
    internal class ResponseValidator
    {
        private readonly AltchaResponseParser _altchaResponseParser;
        private readonly JsonSerializer _serializer;
        private readonly Func<IAltchaChallengeStore> _storeFactory;

        public ResponseValidator(Func<IAltchaChallengeStore> storeFactory,
                                 AltchaResponseParser altchaResponseParser,
                                 JsonSerializer serializer)
        {
            _storeFactory = storeFactory;
            _altchaResponseParser = altchaResponseParser;
            _serializer = serializer;
        }

        public async Task<AltchaValidationResult> Validate(string altchaBase64)
        {
            Guard.NotNullOrWhitespace(altchaBase64);

            var altchaParsedResult = _serializer.FromBase64Json<AltchaResponse>(altchaBase64);
            if (!altchaParsedResult.Success)
                return altchaParsedResult.Error.ToValidationResult();

            return await Validate(altchaParsedResult.Value);
        }

        public async Task<AltchaValidationResult> Validate(AltchaResponse altchaResponse)
        {
            Guard.NotNull(altchaResponse);

            var store = _storeFactory();
            Guard.NotNull<MissingStoreException>(store);

            var validationResult = await IsValidResponse(altchaResponse, store);
            if (!validationResult.Success)
                return validationResult.Error.ToValidationResult();

            var altcha = validationResult.Value;
            await store.Store(altcha.Challenge, altcha.ExpiryUtc);

            return Error.Create(ErrorCode.NoError)
                        .ToValidationResult();
        }

        private async Task<Result<Validation.AltchaResponse>> IsValidResponse(
            AltchaResponse altchaResponse,
            IAltchaChallengeStore store)
        {
            var parseResult = _altchaResponseParser.Parse(altchaResponse);
            if (!parseResult.Success)
                return Result<Validation.AltchaResponse>.Fail(parseResult);

            var altcha = parseResult.Value;
            var exists = await store.Exists(altcha.Challenge);
            if (exists)
                return Result<Validation.AltchaResponse>.Fail(ErrorCode.PreviouslyVerified);

            var validationResult = altcha.Validate();
            return Result<Validation.AltchaResponse>.From(validationResult, altcha);
        }
    }
}
