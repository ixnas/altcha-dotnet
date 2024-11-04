using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ixnas.AltchaNet.Tests.Simulations
{
    internal class AltchaApiSimulation
    {
        [Serializable]
        private class AltchaSpamFiltered
        {
            public string Algorithm { get; set; }
            public string VerificationData { get; set; }
            public string Signature { get; set; }

            public string ToBase64Json()
            {
                var json = JsonSerializer.SerializeToUtf8Bytes(this, TestUtils.JsonSerializerOptions);
                return Convert.ToBase64String(json);
            }
        }

        private readonly string _apiSecret;

        public AltchaApiSimulation(string apiSecret)
        {
            _apiSecret = apiSecret;
        }

        public AltchaChallenge Generate(int expiryOffsetSeconds = 0)
        {
            var nowSeconds = DateTimeOffset.UtcNow.AddSeconds(expiryOffsetSeconds)
                                           .ToUnixTimeSeconds();
            const string randomString = "b9f517664af74946e13c75a5";
            var salt = $"{randomString}?expires={nowSeconds}&_someOtherProperty=true";
            const int randomNumber = 2;
            var key = Encoding.UTF8.GetBytes(_apiSecret);
            using (var hmac = new HMACSHA256(key))
            {
                using (var sha = SHA256.Create())
                {
                    var challengeRaw = Encoding.UTF8.GetBytes($"{salt}{randomNumber}");
                    var challengeBytes = sha.ComputeHash(challengeRaw);
                    var challenge = BitConverter.ToString(challengeBytes)
                                                .Replace("-", string.Empty)
                                                .ToLower();
                    var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(challenge));
                    var signature = BitConverter.ToString(signatureBytes)
                                                .Replace("-", string.Empty)
                                                .ToLower();
                    return new AltchaChallenge
                    {
                        Algorithm = "SHA-256",
                        Challenge = challenge,
                        Maxnumber = 3,
                        Salt = salt,
                        Signature = signature
                    };
                }
            }
        }

        public string GenerateSpamFiltered<T>(T form,
                                              double score,
                                              int expiryOffsetSeconds,
                                              bool verified,
                                              Expression<Func<T, string>> altchaSelector,
                                              Func<string, string> malformSignatureFn = null,
                                              Func<string> replaceAlgorithmFn = null,
                                              Func<string, string> malformVerificationDataFn = null)
        {
            var expiry = DateTimeOffset.UtcNow.AddSeconds(expiryOffsetSeconds)
                                       .ToUnixTimeSeconds();
            var key = Encoding.UTF8.GetBytes(_apiSecret);

            var selector = (MemberExpression)altchaSelector.Body;
            var altchaPropertyName = selector.Member.Name;

            var formProperties = typeof(T).GetProperties()
                                          .Where(property =>
                                                     property.PropertyType == typeof(string)
                                                     && property.Name != altchaPropertyName);
            var propertyDictionary = formProperties.Select(property => new
                                                   {
                                                       Key = property.Name,
                                                       Value = (property.GetValue(form) as string)?.Trim()
                                                   })
                                                   .Where(property => !string.IsNullOrWhiteSpace(
                                                                           property.Value))
                                                   .OrderBy(property => property.Key)
                                                   .ToList();

            var fieldNames = string.Join("%2C", propertyDictionary.Select(property => property.Key));
            var fieldsCombined = string.Join("\n", propertyDictionary.Select(property => property.Value));

            using (var hmac = new HMACSHA256(key))
            {
                using (var sha = SHA256.Create())
                {
                    var fieldsHashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(fieldsCombined));
                    var fieldsHash = BitConverter.ToString(fieldsHashBytes)
                                                 .Replace("-", string.Empty)
                                                 .ToLower();
                    var scoreString = score.ToString(CultureInfo.InvariantCulture);
                    var verifiedString = verified ? "true" : "false";
                    var verificationDataRaw =
                        $"score={scoreString}&fields={fieldNames}&fieldsHash={fieldsHash}&expire={expiry}&verified={verifiedString}";
                    var verificationDataBytes = Encoding.UTF8.GetBytes(verificationDataRaw);
                    var verificationDataHash = sha.ComputeHash(verificationDataBytes);
                    var signatureBytes = hmac.ComputeHash(verificationDataHash);
                    var signature = BitConverter.ToString(signatureBytes)
                                                .Replace("-", string.Empty)
                                                .ToLower();
                    var algorithm = "SHA-256";

                    if (malformSignatureFn != null)
                        signature = malformSignatureFn(signature);

                    if (replaceAlgorithmFn != null)
                        algorithm = replaceAlgorithmFn();

                    if (malformVerificationDataFn != null)
                        verificationDataRaw = malformVerificationDataFn(verificationDataRaw);

                    return new AltchaSpamFiltered
                    {
                        Algorithm = algorithm,
                        VerificationData = verificationDataRaw,
                        Signature = signature
                    }.ToBase64Json();
                }
            }
        }
    }
}
