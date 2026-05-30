using System;
using System.Threading;
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
        private readonly AltchaSha256Configuration _configuration;

        public ResponseValidator(Func<IAltchaChallengeStore> storeFactory,
                                 AltchaResponseParser altchaResponseParser,
                                 JsonSerializer serializer,
                                 AltchaSha256Configuration configuration)
        {
            _storeFactory = storeFactory;
            _altchaResponseParser = altchaResponseParser;
            _serializer = serializer;
            _configuration = configuration;
        }

        public async Task<AltchaValidationResult> Validate(string altchaBase64,
                                                           CancellationToken cancellationToken)
        {
            Guard.NotNullOrWhitespace(altchaBase64);

            var altchaParsedResult = _serializer.FromBase64Json<AltchaResponse>(altchaBase64);
            if (!altchaParsedResult.Success)
                return altchaParsedResult.Error.ToValidationResult();

            return await Validate(altchaParsedResult.Value, cancellationToken);
        }

        public async Task<AltchaValidationResult> Validate(AltchaResponse altchaResponse,
                                                           CancellationToken cancellationToken)
        {
            Guard.NotNull(altchaResponse);

            var store = _storeFactory();
            // stryker disable once nullcoalescing: Fails for .NET Framework
            var storeAdapter = store as ChallengeStoreAdapter ?? new ChallengeStoreAdapter(store);

            var validationResult = await IsValidResponse(altchaResponse, storeAdapter, cancellationToken);
            if (!validationResult.Success)
                return validationResult.Error.ToValidationResult();

            var altcha = validationResult.Value;
            await storeAdapter.Store(altcha.Challenge, altcha.ExpiryUtc, cancellationToken);

            return Error.Create(ErrorCode.NoError)
                        .ToValidationResult();
        }

        private async Task<Result<Validation.AltchaResponse>> IsValidResponse(AltchaResponse altchaResponse,
            ChallengeStoreAdapter store,
            CancellationToken cancellationToken)
        {
            var parseResult = _altchaResponseParser.Parse(altchaResponse);
            if (!parseResult.Success)
                return Result<Validation.AltchaResponse>.Fail(parseResult);

            var altcha = parseResult.Value;
            var exists = await store.Exists(altcha.Challenge, cancellationToken);
            if (exists)
                return Result<Validation.AltchaResponse>.Fail(ErrorCode.PreviouslyVerified);

            var validationResult = altcha.Validate(_configuration.Key);
            return Result<Validation.AltchaResponse>.From(validationResult, altcha);
        }
    }
}
