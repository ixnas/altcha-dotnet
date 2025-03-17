using System;
using System.Threading;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Tests.Fakes;
using Ixnas.AltchaNet.Tests.Simulations;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class SpamFilterTests
    {
        [Serializable]
        private class TestForm : AltchaForm
        {
            public string Email { get; set; }
            public string Text { get; set; }
            public string Something { get; set; }
            public string Altcha { get; set; }
        }

        [Serializable]
        private class TestForm2 : AltchaForm
        {
            public string Email { get; set; }
            public string Text { get; set; }
            public string SomethingElse { get; set; }
            public string Altcha { get; set; }
        }

        [Serializable]
        private class TestFormWithoutAltcha
        {
            public string Email { get; set; }
            public string Something { get; set; }
            public string Text { get; set; }
        }

        [Serializable]
        private class TestFormWithDifferentAltchaProperty
        {
            public string Email { get; set; }
            public string Something { get; set; }
            public string Text { get; set; }
            public string AlternativeAltcha { get; set; }
        }

        private const double DefaultSpamThreshold = 2;
        private readonly AltchaApiSimulation _apiSimulation;

        public SpamFilterTests()
        {
            var key = TestUtils.GetApiSecret();
            _apiSimulation = new AltchaApiSimulation(key);
        }

        [Fact]
        public async Task GivenSpamFilteredFormIsNull_WhenValidateSpamFilteredFormCalled_ThrowsException()
        {
            var service = GetDefaultService();
            await Assert.ThrowsAsync<ArgumentNullException>(() => service
                                                                .ValidateSpamFilteredForm<object>(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("weorijfosdlkjaewropj===")]
        public async Task
            GivenAltchaHasInvalidBase64Format_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
                string altcha)
        {
            const string expectedMessage = "Challenge is not a valid base64 string.";
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.ChallengeIsInvalidBase64;

            var service = GetDefaultService();
            var form = GetDefaultForm();
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedMessage, result.ValidationError.Message);
        }

        [Fact]
        public async Task
            GivenAltchaHasInvalidJsonFormat_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
        {
            const string expectedMessage =
                "Challenge could be base64-decoded, but could not be parsed as JSON.";
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.ChallengeIsInvalidJson;

            const string altcha = "c29tZXRoaW5nIG1pc3NpbmcgaGVyZSBub3c=";
            var service = GetDefaultService();
            var form = GetDefaultForm();
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedMessage, result.ValidationError.Message);
        }

        [Fact]
        public async Task
            GivenFormHasNoAltchaProperty_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
        {
            const string expectedMessage = "Challenge is not a valid base64 string.";
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.ChallengeIsInvalidBase64;

            var service = GetDefaultService();
            var form = new TestFormWithoutAltcha
            {
                Email = "name@provider.com",
                Something = "Some text",
                Text = "More text"
            };
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedMessage, result.ValidationError.Message);
        }

        [Theory]
        [InlineData("Some text", 2)]
        [InlineData("  Some text  ", 2)]
        [InlineData("Some text", 1.5)]
        [InlineData("  Some text  ", 1.5)]
        public async Task GivenFormIsValid_WhenValidateSpamFilteredFormCalled_ReturnsPositiveResult(
            string validText,
            double score)
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.NoError;

            var service = GetDefaultService();
            var form = GetDefaultForm();
            form.Something = validText;
            var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                             score,
                                                             30,
                                                             true,
                                                             f => f.Altcha);
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.True(result.IsValid);
            Assert.True(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(string.Empty, result.ValidationError.Message);
        }

        [Fact]
        public async Task
            GivenFormUsesDifferentAltchaProperty_WhenValidateSpamFilteredFormCalledWithExpression_ReturnsPositiveResult()
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.NoError;

            var service = GetDefaultService();
            var form = new TestFormWithDifferentAltchaProperty
            {
                Email = "name@provider.com",
                Something = "Some text",
                Text = "More text"
            };
            var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                             DefaultSpamThreshold,
                                                             30,
                                                             true,
                                                             f => f.AlternativeAltcha);
            form.AlternativeAltcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form, x => x.AlternativeAltcha);

            Assert.True(result.IsValid);
            Assert.True(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(string.Empty, result.ValidationError.Message);
        }

        [Fact]
        public async Task GivenStoreFactoryProvided_WhenCallingValidateSpamFilteredForm_InstantiatesStore()
        {
            var storeWasInstantiated = false;
            var service = GetServiceWithStoreFactory(() =>
            {
                storeWasInstantiated = true;
                return new AltchaChallengeStoreFake();
            });
            var form = GetDefaultForm();
            var simulation = GenerateDefaultSpamFiltered(form);
            form.Altcha = simulation;

            Assert.False(storeWasInstantiated);
            await service.ValidateSpamFilteredForm(form);
            Assert.True(storeWasInstantiated);
        }

        [Fact]
        public async Task
            GivenStoreFactoryReturnsNull_WhenCallingValidateSpamFilteredForm_ThrowsMissingStoreException()
        {
            var service = GetServiceWithStoreFactory(() => null);
            var form = GetDefaultForm();
            var simulation = GenerateDefaultSpamFiltered(form);
            form.Altcha = simulation;

            await Assert.ThrowsAsync<MissingStoreException>(() => service.ValidateSpamFilteredForm(form));
        }

        [Fact]
        public async Task GivenFormIsValid_WhenValidateSpamFilteredFormCalledTwice_ReturnsNegativeResult()
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.PreviouslyVerified;
            const string expectedErrorString = "Challenge has been verified before.";

            var service = GetDefaultService();
            var form = GetDefaultForm();
            var altcha = GenerateDefaultSpamFiltered(form);
            form.Altcha = altcha;
            await service.ValidateSpamFilteredForm(form);
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedErrorString, result.ValidationError.Message);
        }

        [Fact]
        public async Task GivenFormIsExpired_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.FormSubmissionExpired;
            const string expectedErrorString = "Form submission has expired.";

            var service = GetDefaultService();
            var form = GetDefaultForm();
            var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                             DefaultSpamThreshold,
                                                             -30,
                                                             true,
                                                             f => f.Altcha);
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedErrorString, result.ValidationError.Message);
        }

        [Fact]
        public async Task GivenFormIsNotVerified_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.FormSubmissionNotVerified;
            const string expectedErrorString =
                "Form submission was not successfully verified by ALTCHA's API.";

            var service = GetDefaultService();
            var form = GetDefaultForm();
            var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                             DefaultSpamThreshold,
                                                             30,
                                                             false,
                                                             f => f.Altcha);
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedErrorString, result.ValidationError.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("x")]
        public async Task
            GivenFormHasInvalidSignature_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
                string prefix)
        {
            await TestMalformedAltcha(AltchaSpamFilteredValidationErrorCode.SignatureIsInvalidHexString,
                                      "Signature is not a valid hex string.",
                                      signature => prefix + signature.Substring(1));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GivenFormHasNoSignature_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
            string signature)
        {
            await TestMalformedAltcha(AltchaSpamFilteredValidationErrorCode.SignatureIsInvalidHexString,
                                      "Signature is not a valid hex string.",
                                      _ => signature);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("SHA-512")]
        public async Task
            GivenFormHasInvalidAlgorithm_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
                string signature)
        {
            await TestMalformedAltcha(AltchaSpamFilteredValidationErrorCode.AlgorithmDoesNotMatch,
                                      "Algorithm does not match the algorithm that was configured.",
                                      null,
                                      () => signature);
        }

        [Fact]
        public async Task
            GivenFormHasMalformedVerificationData_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
        {
            await TestMalformedAltcha(AltchaSpamFilteredValidationErrorCode.PayloadDoesNotMatchSignature,
                                      "Payload does not match signature.",
                                      null,
                                      null,
                                      verificationData =>
                                          verificationData.Replace("score=2", "score=x"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task
            GivenFormHasEmptyVerificationData_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
                string verificationData)
        {
            await TestMalformedAltcha(AltchaSpamFilteredValidationErrorCode.PayloadDoesNotMatchSignature,
                                      "Payload does not match signature.",
                                      null,
                                      null,
                                      _ => verificationData);
        }

        [Fact]
        public async Task GivenFormHasSpam_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.NoError;

            var service = GetDefaultService();
            var form = GetDefaultForm();
            var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                             DefaultSpamThreshold + 0.1,
                                                             30,
                                                             true,
                                                             f => f.Altcha);
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.True(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(string.Empty, result.ValidationError.Message);
        }

        [Fact]
        public async Task
            GivenFormHasSpamAccordingToCustomThreshold_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.NoError;

            var service = GetServiceWithThreshold(0.5);
            var form = GetDefaultForm();
            var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                             0.6,
                                                             30,
                                                             true,
                                                             f => f.Altcha);
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.True(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(string.Empty, result.ValidationError.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task
            GivenFormHasEmptyButMatchingField_WhenValidateSpamFilteredFormCalled_ReturnsPositiveResult(
                string emptyField)
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.NoError;

            var service = GetDefaultService();
            var form = GetDefaultForm();
            form.Something = emptyField;
            var altcha = GenerateDefaultSpamFiltered(form);
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.True(result.IsValid);
            Assert.True(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(string.Empty, result.ValidationError.Message);
        }

        [Fact]
        public async Task
            GivenFormIsHasAlteredFieldValue_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.FormFieldValuesDontMatch;
            const string expectedErrorString =
                "This form's field values do not match what was verified by ALTCHA API's spam filter.";

            var service = GetDefaultService();
            var form = GetDefaultForm();
            form.Something = "Before";
            var altcha = GenerateDefaultSpamFiltered(form);

            form.Altcha = altcha;
            form.Something = "After";
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedErrorString, result.ValidationError.Message);
        }

        [Theory]
        [InlineData("Some text", "Some text")]
        [InlineData("Some text", "Something else")]
        [InlineData("Some text", null)]
        [InlineData(null, "Some text")]
        public async Task GivenFormIsHasAlteredField_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
            string before,
            string after)
        {
            const AltchaSpamFilteredValidationErrorCode expectedErrorCode =
                AltchaSpamFilteredValidationErrorCode.FormFieldsDontMatch;
            const string expectedErrorString =
                "This form's fields do not match what was verified by ALTCHA API's spam filter.";

            var service = GetDefaultService();
            var form1 = GetDefaultForm();
            form1.Something = before;
            var form2 = new TestForm2
            {
                Email = "name@provider.com",
                SomethingElse = after,
                Text = "More text"
            };
            var altcha = GenerateDefaultSpamFiltered(form1);
            form2.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form2);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedErrorString, result.ValidationError.Message);
        }

        [Theory]
        [InlineData(CancellationMethod.Store)]
        [InlineData(CancellationMethod.Exists)]
        public async Task GivenCancellationTokenIsPassed_WhenValidateCanceled_ThenStoreCanCancel(
            CancellationMethod cancellationMethod)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var store = new AltchaChallengeStoreFake
            {
                CancellationSimulation = cancellationMethod
            };
            var service = GetServiceWithStoreFactory(() => store);
            var form = GetDefaultForm();
            var altcha = GenerateDefaultSpamFiltered(GetDefaultForm());
            form.Altcha = altcha;
            // ReSharper disable once MethodSupportsCancellation
            var task = Task.Run(async () =>
                                    await service.ValidateSpamFilteredForm(form,
                                             cancellationTokenSource.Token));
            // ReSharper disable once MethodHasAsyncOverload
            cancellationTokenSource.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        }

        [Fact]
        public async Task GivenCancellationTokenIsPassed_WhenValidateIsNotCanceled_ThenReturnResult()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var store = new AltchaChallengeStoreFake();
            var service = GetServiceWithStoreFactory(() => store);
            var form = GetDefaultForm();
            var altcha = GenerateDefaultSpamFiltered(GetDefaultForm());
            form.Altcha = altcha;
            var validationResult = await service.ValidateSpamFilteredForm(form,
                                            cancellationTokenSource.Token);
            Assert.True(validationResult.IsValid);
        }

        private async Task TestMalformedAltcha(AltchaSpamFilteredValidationErrorCode expectedErrorCode,
                                               string expectedErrorMessage,
                                               Func<string, string> malformedSignatureFn = null,
                                               Func<string> replaceAlgorithmFn = null,
                                               Func<string, string> malformVerificationDataFn = null)
        {
            var service = GetDefaultService();
            var form = GetDefaultForm();
            var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                             DefaultSpamThreshold,
                                                             30,
                                                             true,
                                                             f => f.Altcha,
                                                             malformedSignatureFn,
                                                             replaceAlgorithmFn,
                                                             malformVerificationDataFn);
            form.Altcha = altcha;
            var result = await service.ValidateSpamFilteredForm(form);

            Assert.False(result.IsValid);
            Assert.False(result.PassedSpamFilter);

            Assert.Equal(expectedErrorCode, result.ValidationError.Code);
            Assert.Equal(expectedErrorMessage, result.ValidationError.Message);
        }

        private static TestForm GetDefaultForm()
        {
            return new TestForm
            {
                Email = "name@provider.com",
                Something = "Some text",
                Text = "More text"
            };
        }

        private string GenerateDefaultSpamFiltered<T>(T form) where T : AltchaForm
        {
            return _apiSimulation.GenerateSpamFiltered(form,
                                                       DefaultSpamThreshold,
                                                       30,
                                                       true,
                                                       f => f.Altcha);
        }

        private static AltchaApiService GetDefaultService()
        {
            return Altcha.CreateApiServiceBuilder()
                         .UseInMemoryStore()
                         .UseApiSecret(TestUtils.GetApiSecret())
                         .Build();
        }

        private static AltchaApiService GetServiceWithThreshold(double threshold)
        {
            return Altcha.CreateApiServiceBuilder()
                         .UseInMemoryStore()
                         .UseApiSecret(TestUtils.GetApiSecret())
                         .SetMaxSpamFilterScore(threshold)
                         .Build();
        }

        private static AltchaApiService GetServiceWithStoreFactory(
            Func<IAltchaCancellableChallengeStore> storeFactory)
        {
            return Altcha.CreateApiServiceBuilder()
                         .UseStore(storeFactory)
                         .UseApiSecret(TestUtils.GetApiSecret())
                         .Build();
        }
    }
}
