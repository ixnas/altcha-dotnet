using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Internal.Converters;
using Ixnas.AltchaNet.Internal.Cryptography;
using Ixnas.AltchaNet.Internal.Serialization;

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

        private readonly BytesStringConverter _bytesStringConverter;
        private readonly Clock _clock;
        private readonly CryptoAlgorithm _cryptoAlgorithm;
        private readonly double _maxSpamFilterScore;
        private readonly JsonSerializer _serializer;
        private readonly IAltchaChallengeStore _store;

        public SpamFilterValidator(JsonSerializer serializer,
                                   CryptoAlgorithm cryptoAlgorithm,
                                   BytesStringConverter bytesStringConverter,
                                   Clock clock,
                                   IAltchaChallengeStore store,
                                   double maxSpamFilterScore)
        {
            _serializer = serializer;
            _cryptoAlgorithm = cryptoAlgorithm;
            _bytesStringConverter = bytesStringConverter;
            _clock = clock;
            _store = store;
            _maxSpamFilterScore = maxSpamFilterScore;
        }

        public async Task<AltchaSpamFilteredValidationResult> ValidateSpamFilteredForm<T>(
            T form,
            Expression<Func<T, string>> altchaSelector)
        {
            if (form == null)
                throw new ArgumentNullException();
            var parsedForm = ParseForm(form, altchaSelector);

            SpamFilterVerificationData verificationData = null;
            var isValid = TryDeserializeAltcha(parsedForm.Altcha, out var altcha)
                          && AlgorithmMatches(altcha.Algorithm)
                          && SignatureIsValid(altcha.Signature, altcha.VerificationData)
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

        private bool SignatureIsValid(string signature, string verificationData)
        {
            if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(verificationData))
                return false;

            var signatureResult = _bytesStringConverter.GetByteArrayFromHexString(signature);
            if (!signatureResult.Success)
                return false;

            var verificationDataHash =
                _cryptoAlgorithm.GetHash(_bytesStringConverter.GetByteArrayFromUtf8String(verificationData));
            var verificationDataHashSigned = _cryptoAlgorithm.GetSignature(verificationDataHash);

            return verificationDataHashSigned.SequenceEqual(signatureResult.Value);
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
            return _clock.GetUtcNow() < timestamp;
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
                _bytesStringConverter.GetHexStringFromBytes(_cryptoAlgorithm.GetHash(_bytesStringConverter
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
