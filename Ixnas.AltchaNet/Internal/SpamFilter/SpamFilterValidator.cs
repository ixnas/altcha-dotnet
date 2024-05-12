using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using Ixnas.AltchaNet.Debug;
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
        private readonly IAltchaChallengeStore _store;

        public SpamFilterValidator(JsonSerializer serializer,
                                   CryptoAlgorithm cryptoAlgorithm,
                                   Clock clock,
                                   IAltchaChallengeStore store,
                                   SignatureParser signatureParser,
                                   double maxSpamFilterScore)
        {
            _serializer = serializer;
            _cryptoAlgorithm = cryptoAlgorithm;
            _clock = clock;
            _store = store;
            _maxSpamFilterScore = maxSpamFilterScore;
            _signatureParser = signatureParser;
        }

        public async Task<AltchaSpamFilteredValidationResult> ValidateSpamFilteredForm<T>(
            T form,
            Expression<Func<T, string>> altchaSelector)
        {
            Guard.NotNull(form);
            var parsedForm = ParseForm(form, altchaSelector);

            SpamFilterVerificationData verificationData = null;
            var isValid = TryDeserializeAltcha(parsedForm.Altcha, out var altcha)
                          && AlgorithmMatches(altcha.Algorithm)
                          && _signatureParser.TryParse(altcha.Signature, out var signature)
                          && signature.PayloadIsValid(altcha.VerificationData)
                          && await ChallengeIsNew(altcha.VerificationData)
                          && TryParseVerificationData(altcha.VerificationData, out verificationData)
                          && verificationData.Verified
                          && TimestampIsValid(verificationData.ExpiryUtc)
                          && FormFieldsMatch(parsedForm, verificationData);

            if (!isValid)
                return new AltchaSpamFilteredValidationResult();

            await _store.Store(altcha.VerificationData, verificationData.ExpiryUtc);

            if (!PassesSpamFilter(verificationData))
                return new AltchaSpamFilteredValidationResult
                {
                    IsValid = true
                };

            return new AltchaSpamFilteredValidationResult
            {
                IsValid = true,
                PassedSpamFilter = true
            };
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
                                   Key = property.Name[0]
                                                 .ToString()
                                                 .ToLower()
                                         + property.Name.Substring(1),
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

        private bool TryDeserializeAltcha(string altchaString, out SpamFilteredAltcha altcha)
        {
            if (string.IsNullOrWhiteSpace(altchaString))
            {
                altcha = null;
                return false;
            }

            var deserialized = _serializer.FromBase64Json<SpamFilteredAltcha>(altchaString);
            if (!deserialized.Success)
            {
                altcha = null;
                return false;
            }

            altcha = deserialized.Value;
            return true;
        }

        private bool AlgorithmMatches(string algorithm)
        {
            return _cryptoAlgorithm.Name == algorithm;
        }

        private async Task<bool> ChallengeIsNew(string altchaVerificationData)
        {
            return !await _store.Exists(altchaVerificationData);
        }

        private static bool TryParseVerificationData(string verificationDataString,
                                                     out SpamFilterVerificationData verificationData)
        {
            var dictionary = HttpUtility.ParseQueryString(verificationDataString);
            var fieldNames = dictionary["fields"]
                             .Split(',')
                             .ToList();
            var fieldsHash = dictionary["fieldsHash"];
            var score = double.Parse(dictionary["score"], CultureInfo.InvariantCulture);
            var expiryUtcSeconds = int.Parse(dictionary["expire"]);
            var expiryUtc = DateTimeOffset.FromUnixTimeSeconds(expiryUtcSeconds);
            var verified = dictionary["verified"]
                .Equals("true", StringComparison.OrdinalIgnoreCase);

            verificationData = new SpamFilterVerificationData
            {
                FieldNames = fieldNames,
                FieldHash = fieldsHash,
                Score = score,
                ExpiryUtc = expiryUtc,
                Verified = verified
            };

            return true;
        }

        private bool TimestampIsValid(DateTimeOffset timestamp)
        {
            // stryker disable once equality: Impossible to black box test to exactly now.
            return _clock.UtcNow < timestamp;
        }

        private bool FormFieldsMatch(Form parsedForm, SpamFilterVerificationData verificationData)
        {
            var fieldNames = verificationData.FieldNames;

            var fieldsToHash = parsedForm.Fields;
            fieldsToHash.Sort((a, b) => fieldNames.IndexOf(a.Key) - fieldNames.IndexOf(b.Key));

            if (!fieldsToHash.Select(field => field.Key)
                             .SequenceEqual(fieldNames))
                return false;

            var combinedFields = string.Join("\n", fieldsToHash.Select(field => field.Value));
            var calculatedHash =
                ByteConverter.GetHexStringFromBytes(_cryptoAlgorithm.Hash(ByteConverter
                                                                        .GetByteArrayFromUtf8String(combinedFields)));

            var fieldsHash = verificationData.FieldHash;
            return calculatedHash == fieldsHash;
        }

        private bool PassesSpamFilter(SpamFilterVerificationData verificationData)
        {
            return verificationData.Score <= _maxSpamFilterScore;
        }
    }
}
