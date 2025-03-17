using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Internal.Common.Converters;
using Ixnas.AltchaNet.Internal.Common.Cryptography;
using Ixnas.AltchaNet.Internal.Common.Serialization;
using Ixnas.AltchaNet.Internal.Common.Utilities;

namespace Ixnas.AltchaNet.Internal.SpamFilter
{
    internal class SpamFilterValidator
    {
        [Serializable]
        private class SpamFilteredAltcha
        {
            public string Algorithm { get; set; }
            public string VerificationData { get; set; }
            public string Signature { get; set; }
        }

        private class Form
        {
            public class FormField
            {
                public string Key { get; set; }
                public string Value { get; set; }
            }

            public string Altcha { get; set; }
            public List<FormField> Fields { get; set; } = new List<FormField>();
        }

        private class SpamFilterVerificationData
        {
            public List<string> FieldNames { get; set; } = new List<string>();
            public string FieldHash { get; set; }
            public double Score { get; set; }
            public DateTimeOffset ExpiryUtc { get; set; }
            public bool Verified { get; set; }
        }

        private readonly Clock _clock;
        private readonly CryptoAlgorithm _cryptoAlgorithm;
        private readonly double _maxSpamFilterScore;
        private readonly JsonSerializer _serializer;
        private readonly SignatureParser _signatureParser;
        private readonly Func<IAltchaCancellableChallengeStore> _storeFactory;

        public SpamFilterValidator(JsonSerializer serializer,
                                   CryptoAlgorithm cryptoAlgorithm,
                                   Clock clock,
                                   Func<IAltchaCancellableChallengeStore> storeFactory,
                                   SignatureParser signatureParser,
                                   double maxSpamFilterScore)
        {
            _serializer = serializer;
            _cryptoAlgorithm = cryptoAlgorithm;
            _clock = clock;
            _storeFactory = storeFactory;
            _maxSpamFilterScore = maxSpamFilterScore;
            _signatureParser = signatureParser;
        }

        public async Task<AltchaSpamFilteredValidationResult> ValidateSpamFilteredForm<T>(
            T form,
            Expression<Func<T, string>> altchaSelector,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(form);

            var store = _storeFactory();
            Guard.NotNull<MissingStoreException>(store);

            var validationResult = await Validate(form, altchaSelector, store, cancellationToken);
            if (!validationResult.Success)
                return validationResult.Error.ToSpamFilteredValidationResult(false);

            var altcha = validationResult.Value.Altcha;
            var verificationData = validationResult.Value.VerificationData;

            await store.Store(altcha.VerificationData, verificationData.ExpiryUtc, cancellationToken);

            if (!PassesSpamFilter(verificationData))
                return Error.Create(ErrorCode.NoError)
                            .ToSpamFilteredValidationResult(false);

            return Error.Create(ErrorCode.NoError)
                        .ToSpamFilteredValidationResult(true);
        }

        private async Task<Result<(SpamFilteredAltcha Altcha, SpamFilterVerificationData VerificationData)>>
            Validate<T>(T form,
                        Expression<Func<T, string>> altchaSelector,
                        IAltchaCancellableChallengeStore store,
                        CancellationToken cancellationToken)
        {
            var parsedForm = ParseForm(form, altchaSelector);
            return (await new Railway<SpamFilteredAltcha>(DeserializeAltcha(parsedForm.Altcha))
                          .Then(AlgorithmMatches)
                          .Then(TryParseSignature)
                          .Then(PayloadIsValid)
                          .ThenAsync(altcha => ChallengeIsNew(store, altcha, cancellationToken)))
                   .Then(ParseVerificationData)
                   .Then(CheckExpiry)
                   .Then(prev => CheckFormFields(parsedForm, prev.Altcha, prev.VerificationData))
                   .Result;
        }

        private Result<(SpamFilteredAltcha Altcha, Signature Signature)> TryParseSignature(
            SpamFilteredAltcha altcha)
        {
            var parseResult = _signatureParser.Parse(altcha.Signature);
            return Result<(SpamFilteredAltcha, Signature)>.From(parseResult,
                                                                signature => (altcha, signature));
        }

        private Result<SpamFilteredAltcha> PayloadIsValid((SpamFilteredAltcha, Signature) parameters)
        {
            var (altcha, signature) = parameters;
            var validationResult = signature.PayloadIsValid(altcha.VerificationData);
            return Result<SpamFilteredAltcha>.From(validationResult, altcha);
        }

        private static Form ParseForm<T>(T form, Expression<Func<T, string>> altchaSelector)
        {
            var altchaPropertyName = GetAltchaPropertyName(altchaSelector);
            var properties = typeof(T).GetProperties()
                                      .Where(property => property.PropertyType == typeof(string))
                                      .ToList();
            var altchaBase64 = properties.SingleOrDefault(property => property.Name == altchaPropertyName)
                                         ?.GetValue(form) as string;
            var fieldsToHash = properties
                               .Where(property =>
                                          property.Name != altchaPropertyName)
                               .Select(property => new Form.FormField
                               {
                                   Key = property.Name,
                                   Value = (property.GetValue(form) as string)?.Trim()
                               })
                               .Where(property => !string.IsNullOrWhiteSpace(property.Value))
                               .ToList();

            return new Form
            {
                Altcha = altchaBase64,
                Fields = fieldsToHash
            };
        }

        private static string GetAltchaPropertyName<T>(Expression<Func<T, string>> altchaSelector)
        {
            if (altchaSelector == null)
                return Defaults.AltchaPropertyName;

            var selector = (MemberExpression)altchaSelector.Body;
            return selector.Member.Name;
        }

        private Result<SpamFilteredAltcha> DeserializeAltcha(string altchaString)
        {
            if (string.IsNullOrWhiteSpace(altchaString))
                return Result<SpamFilteredAltcha>.Fail(ErrorCode.ChallengeIsInvalidBase64);

            return _serializer.FromBase64Json<SpamFilteredAltcha>(altchaString);
        }

        private Result<SpamFilteredAltcha> AlgorithmMatches(SpamFilteredAltcha altcha)
        {
            if (_cryptoAlgorithm.Name != altcha.Algorithm)
                return Result<SpamFilteredAltcha>.Fail(ErrorCode.AlgorithmDoesNotMatch);
            return Result<SpamFilteredAltcha>.Ok(altcha);
        }

        private async static Task<Result<SpamFilteredAltcha>> ChallengeIsNew(
            IAltchaCancellableChallengeStore store,
            SpamFilteredAltcha altcha,
            CancellationToken cancellationToken)
        {
            var exists = await store.Exists(altcha.VerificationData, cancellationToken);
            if (exists)
                return Result<SpamFilteredAltcha>.Fail(ErrorCode.PreviouslyVerified);
            return Result<SpamFilteredAltcha>.Ok(altcha);
        }

        private static Result<(SpamFilteredAltcha Altcha, SpamFilterVerificationData VerificationData)>
            ParseVerificationData(SpamFilteredAltcha altcha)
        {
            var dictionary = HttpUtility.ParseQueryString(altcha.VerificationData);
            var fieldNames = dictionary["fields"]
                             .Split(',')
                             .ToList();
            var fieldsHash = dictionary["fieldsHash"];
            var score = double.Parse(dictionary["score"], CultureInfo.InvariantCulture);
            var expiryUtcSeconds = int.Parse(dictionary["expire"]);
            var expiryUtc = DateTimeOffset.FromUnixTimeSeconds(expiryUtcSeconds);
            var verified = dictionary["verified"]
                .Equals("true", StringComparison.OrdinalIgnoreCase);

            if (!verified)
                return Result<(SpamFilteredAltcha Altcha, SpamFilterVerificationData VerificationData)>
                    .Fail(ErrorCode.FormSubmissionNotVerified);

            var verificationData = new SpamFilterVerificationData
            {
                FieldNames = fieldNames,
                FieldHash = fieldsHash,
                Score = score,
                ExpiryUtc = expiryUtc,
                Verified = verified
            };

            return Result<(SpamFilteredAltcha Altcha, SpamFilterVerificationData VerificationData)>
                .Ok((altcha, verificationData));
        }

        private Result<(SpamFilteredAltcha Altcha, SpamFilterVerificationData VerificationData)> CheckExpiry(
            (SpamFilteredAltcha, SpamFilterVerificationData) parameters)
        {
            var (altcha, verificationData) = parameters;
            var timestamp = verificationData.ExpiryUtc;

            // stryker disable once equality: Impossible to black box test to exactly now.
            var isValid = _clock.UtcNow < timestamp;
            if (!isValid)
                return Result<(SpamFilteredAltcha, SpamFilterVerificationData)>.Fail(ErrorCode
                             .FormSubmissionExpired);

            return Result<(SpamFilteredAltcha, SpamFilterVerificationData)>.Ok((altcha, verificationData));
        }

        private Result<(SpamFilteredAltcha Altcha, SpamFilterVerificationData VerificationData)>
            CheckFormFields(
                Form parsedForm,
                SpamFilteredAltcha altcha,
                SpamFilterVerificationData verificationData)
        {
            var fieldNames = verificationData.FieldNames;

            var fieldsToHash = parsedForm.Fields;
            fieldsToHash.Sort((a, b) => fieldNames.IndexOf(a.Key) - fieldNames.IndexOf(b.Key));

            if (!fieldsToHash.Select(field => field.Key)
                             .SequenceEqual(fieldNames))
                return Result<(SpamFilteredAltcha, SpamFilterVerificationData)>.Fail(ErrorCode
                             .FormFieldsDontMatch);

            var combinedFields = string.Join("\n", fieldsToHash.Select(field => field.Value));
            var calculatedHash =
                ByteConverter.GetHexStringFromBytes(_cryptoAlgorithm.Hash(ByteConverter
                                                                 .GetByteArrayFromUtf8String(combinedFields)));

            var fieldsHash = verificationData.FieldHash;
            var fieldValuesMatch = calculatedHash == fieldsHash;
            if (!fieldValuesMatch)
                return Result<(SpamFilteredAltcha, SpamFilterVerificationData)>.Fail(ErrorCode
                             .FormFieldValuesDontMatch);
            return Result<(SpamFilteredAltcha, SpamFilterVerificationData)>.Ok((altcha, verificationData));
        }

        private bool PassesSpamFilter(SpamFilterVerificationData verificationData)
        {
            return verificationData.Score <= _maxSpamFilterScore;
        }
    }
}
