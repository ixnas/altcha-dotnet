using System;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Tests.Abstractions;
using Ixnas.AltchaNet.Tests.Fakes;
using Ixnas.AltchaNet.Tests.Simulations;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class CommonChallengeTests
    {
        private readonly ClockFake _clock = new ClockFake();

        [Theory]
        [InlineData(null, CommonServiceType.Default)]
        [InlineData("", CommonServiceType.Default)]
        [InlineData(" ", CommonServiceType.Default)]
        [InlineData(null, CommonServiceType.Api)]
        [InlineData("", CommonServiceType.Api)]
        [InlineData(" ", CommonServiceType.Api)]
        public async Task GivenChallengeIsEmpty_WhenValidateCalled_ThenThrowException(
            string altcha,
            CommonServiceType commonServiceType)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            var responseSet = new AltchaResponseSet
            {
                Base64 = altcha
            };
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.Validate(responseSet,
                                                                CommonServiceValidationMethod.Base64));
        }

        [Theory]
        [InlineData(CommonServiceType.Default)]
        [InlineData(CommonServiceType.Api)]
        public async Task GivenChallengeObjectIsNull_WhenValidateCalled_ThenThrowException(
            CommonServiceType commonServiceType)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            var responseSet = new AltchaResponseSet
            {
                Object = null
            };
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.Validate(responseSet,
                                                                CommonServiceValidationMethod.Object));
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenChallengeIsSolved_WhenCallingValidate_ReturnsPositiveResult(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            var challenge = service.Generate();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);
            var validationResult = await service.Validate(result.Altcha, validationMethod);

            Assert.True(result.Succeeded);
            Assert.True(validationResult.IsValid);

            Assert.Equal(AltchaValidationErrorCode.NoError, validationResult.ValidationError.Code);
            Assert.Equal(string.Empty, validationResult.ValidationError.Message);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenStoreFactoryProvided_WhenCallingValidate_InstantiatesStore(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var storeWasInstantiated = false;
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetServiceWithStoreFactory(() =>
                                   {
                                       storeWasInstantiated = true;
                                       return new AltchaChallengeStoreFake();
                                   });
            var challenge = service.Generate();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);

            Assert.False(storeWasInstantiated);
            await service.Validate(result.Altcha, validationMethod);
            Assert.True(storeWasInstantiated);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenStoreFactoryReturnsNull_WhenCallingValidate_ThrowsMissingStoreException(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetServiceWithStoreFactory(() => null);
            var challenge = service.Generate();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);
            await Assert.ThrowsAsync<MissingStoreException>(() => service.Validate(result.Altcha,
                                                                validationMethod));
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenChallengedIsSolvedAfterExpiry_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.ChallengeExpired;
            const string expectedErrorString = "Challenge expired.";

            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetServiceWithExpiry(1, null, _clock);
            var challenge = service.Generate();
            _clock.SetOffsetInSeconds(2);
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);
            var validationResult = await service.Validate(result.Altcha, validationMethod);

            Assert.True(result.Succeeded);
            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedErrorCode, validationResult.ValidationError.Code);
            Assert.Equal(expectedErrorString, validationResult.ValidationError.Message);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task
            GivenChallengeIsSolvedWithOldService_WhenCallingValidateOnNewService_RespectsOldExpiry(
                CommonServiceType commonServiceType,
                CommonServiceValidationMethod validationMethod)
        {
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.ChallengeExpired;
            const string expectedErrorString = "Challenge expired.";

            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetServiceWithExpiry(1, null, _clock);
            var challenge = service.Generate();
            _clock.SetOffsetInSeconds(2);
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);
            var newService = TestUtils.ServiceFactories[commonServiceType]
                                      .GetServiceWithExpiry(30, null, _clock);
            var validationResult = await newService.Validate(result.Altcha, validationMethod);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedErrorCode, validationResult.ValidationError.Code);
            Assert.Equal(expectedErrorString, validationResult.ValidationError.Message);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenChallengedHasExpiry_WhenCallingValidate_StoresMatchingExpiry(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var store = new AltchaChallengeStoreFake();
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetServiceWithExpiry(30, store);
            var challenge = service.Generate();
            var tenSecondsFromNow = DateTimeOffset.UtcNow.AddSeconds(30);
            var marginStart = tenSecondsFromNow.AddSeconds(-2);
            var marginEnd = tenSecondsFromNow.AddSeconds(2);

            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);
            await service.Validate(result.Altcha, validationMethod);

            var expiry = store.Stored.Value.Expiry;
            Assert.InRange(expiry, marginStart, marginEnd);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenChallengeIsSolved_WhenCallingValidateTwice_ReturnsNegativeResult(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.PreviouslyVerified;
            const string expectedErrorString = "Challenge has been verified before.";

            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            var challenge = service.Generate();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);
            await service.Validate(result.Altcha, validationMethod);
            var validationResult = await service.Validate(result.Altcha, validationMethod);

            Assert.True(result.Succeeded);
            Assert.False(validationResult.IsValid);

            Assert.Equal(expectedErrorCode, validationResult.ValidationError.Code);
            Assert.Equal(expectedErrorString, validationResult.ValidationError.Message);
        }

        [Theory]
        [InlineData("", CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData("", CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        [InlineData("", CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData("", CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData("x", CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData("x", CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        [InlineData("x", CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData("x", CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        public async Task GivenMalformedSignature_WhenCallingValidate_ReturnsNegativeResult(
            string prefix,
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            await TestMalformedSimulation(service,
                                          signature => prefix + signature.Substring(1),
                                          null,
                                          null,
                                          null,
                                          AltchaValidationErrorCode.SignatureIsInvalidHexString,
                                          "Signature is not a valid hex string.",
                                          validationMethod);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenWrongSignature_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            await TestMalformedSimulation(service,
                                          signature => "aaaaaa" + signature.Substring(6),
                                          null,
                                          null,
                                          null,
                                          AltchaValidationErrorCode.PayloadDoesNotMatchSignature,
                                          "Payload does not match signature.",
                                          validationMethod);
        }

        [Theory]
        [InlineData("", CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData("", CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        [InlineData("", CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData("", CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData("x", CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData("x", CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        [InlineData("x", CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData("x", CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        public async Task GivenMalformedChallenge_WhenCallingValidate_ReturnsNegativeResult(
            string prefix,
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            await TestMalformedSimulation(service,
                                          null,
                                          challenge => prefix + challenge.Substring(1),
                                          null,
                                          null,
                                          AltchaValidationErrorCode.ChallengeDoesNotMatch,
                                          "Calculated salt and secret number combination does not match the challenge.",
                                          validationMethod);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenWrongSecretNumber_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            await TestMalformedSimulation(service,
                                          null,
                                          null,
                                          () => -1,
                                          null,
                                          AltchaValidationErrorCode.ChallengeDoesNotMatch,
                                          "Calculated salt and secret number combination does not match the challenge.",
                                          validationMethod);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task GivenWrongAlgorithm_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceType commonServiceType,
            CommonServiceValidationMethod validationMethod)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            await TestMalformedSimulation(service,
                                          null,
                                          null,
                                          null,
                                          () => "SHA-512",
                                          AltchaValidationErrorCode.AlgorithmDoesNotMatch,
                                          "Algorithm does not match the algorithm that was configured.",
                                          validationMethod);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, "", CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, null, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, "weirojoij", CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default,
                    "eyJzb21ldGhpbmciOiJlbHNlIiwiaXNudCI6InJpZ2h0In0=",
                    CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, "", CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, null, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, "weirojoij", CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, "iowjeroij.jwojeorij", CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, "iowjeroij?jwojeorij", CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api,
                    "iowjeroij?expires=oijewr34",
                    CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, "", CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Default, null, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Default, "weirojoij", CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Default,
                    "eyJzb21ldGhpbmciOiJlbHNlIiwiaXNudCI6InJpZ2h0In0=",
                    CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, "", CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, null, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, "weirojoij", CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, "iowjeroij.jwojeorij", CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, "iowjeroij?jwojeorij", CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api,
                    "iowjeroij?expires=oijewr34",
                    CommonServiceValidationMethod.Object)]
        public async Task GivenMalformedSalt_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceType commonServiceType,
            string malformedSalt,
            CommonServiceValidationMethod validationMethod)
        {
            const string expectedMessage = "Salt does not have the expected format.";
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.InvalidSalt;
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            var challenge = service.Generate();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge,
                                        null,
                                        null,
                                        _ => malformedSalt);
            var run = await service.Validate(result.Altcha, validationMethod);

            Assert.False(run.IsValid);
            Assert.Equal(expectedErrorCode, run.ValidationError.Code);
            Assert.Equal(expectedMessage, run.ValidationError.Message);
        }

        [Theory]
        [InlineData(CommonServiceType.Default)]
        [InlineData(CommonServiceType.Api)]
        public async Task GivenMalformedAltchaBase64_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceType commonServiceType)
        {
            const string malformedAltcha64 = "weirojoij";
            const string expectedMessage = "Challenge is not a valid base64 string.";
            const AltchaValidationErrorCode expectedErrorCode =
                AltchaValidationErrorCode.ChallengeIsInvalidBase64;
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            var responseSet = new AltchaResponseSet
            {
                Base64 = malformedAltcha64
            };
            var run = await service.Validate(responseSet, CommonServiceValidationMethod.Base64);
            Assert.False(run.IsValid);
            Assert.Equal(expectedErrorCode, run.ValidationError.Code);
            Assert.Equal(expectedMessage, run.ValidationError.Message);
        }

        [Theory]
        [InlineData(CommonServiceType.Default)]
        [InlineData(CommonServiceType.Api)]
        public async Task GivenMalformedAltchaBase64Json_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceType commonServiceType)
        {
            const string malformedAltcha64 = "bm90IGEganNvbiBzdHJpbmc=";
            const string expectedMessage =
                "Challenge could be base64-decoded, but could not be parsed as JSON.";
            const AltchaValidationErrorCode expectedErrorCode =
                AltchaValidationErrorCode.ChallengeIsInvalidJson;
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetDefaultService();
            var responseSet = new AltchaResponseSet
            {
                Base64 = malformedAltcha64
            };
            var run = await service.Validate(responseSet, CommonServiceValidationMethod.Base64);
            Assert.False(run.IsValid);
            Assert.Equal(expectedErrorCode, run.ValidationError.Code);
            Assert.Equal(expectedMessage, run.ValidationError.Message);
        }

        [Theory]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Default, CommonServiceValidationMethod.Object)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceType.Api, CommonServiceValidationMethod.Object)]
        public async Task
            GivenStoredChallengesAreExpired_WhenChallengeIsValidated_CleansExpiredChallengesFromInMemoryStore(
                CommonServiceType commonServiceType,
                CommonServiceValidationMethod validationMethod)
        {
            var service = TestUtils.ServiceFactories[commonServiceType]
                                   .GetServiceWithExpiry(20, null, _clock);
            var challenge = service.Generate();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);

            var run1 = await service.Validate(result.Altcha, validationMethod);
            Assert.True(run1.IsValid);

            _clock.SetOffsetInSeconds(40);
            var run2 = await service.Validate(result.Altcha, validationMethod); // cleaned
            Assert.False(run2.IsValid);

            _clock.SetOffsetInSeconds(0);
            var run3 = await service.Validate(result.Altcha, validationMethod);
            Assert.True(run3.IsValid);
        }

        private async static Task TestMalformedSimulation(CommonService service,
                                                          Func<string, string> malformSignatureFn,
                                                          Func<string, string> malformChallengeFn,
                                                          Func<int> replaceSecretNumberFn,
                                                          Func<string> replaceAlgorithmFn,
                                                          AltchaValidationErrorCode expectedErrorCode,
                                                          string expectedErrorMessage,
                                                          CommonServiceValidationMethod validationMethod)
        {
            var challenge = service.Generate();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge,
                                        malformSignatureFn,
                                        malformChallengeFn,
                                        null,
                                        replaceSecretNumberFn,
                                        replaceAlgorithmFn);
            var validationResult = await service.Validate(result.Altcha, validationMethod);

            Assert.True(result.Succeeded);
            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedErrorCode, validationResult.ValidationError.Code);
            Assert.Equal(expectedErrorMessage, validationResult.ValidationError.Message);
        }
    }
}
