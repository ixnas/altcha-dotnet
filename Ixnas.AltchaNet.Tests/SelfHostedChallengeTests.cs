using System;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Tests.Abstractions;
using Ixnas.AltchaNet.Tests.Fakes;
using Ixnas.AltchaNet.Tests.Simulations;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class SelfHostedChallengeTests
    {
        public enum OverrideMethod
        {
            Deprecated,
            ConfigurationRecord
        }

        private readonly ClockFake _clock = new ClockFake();

        [Fact]
        public void WhenGenerateCalled_ThenChallengeNotNull()
        {
            var service = GetDefaultService();
            var challenge = service.Generate();
            Assert.NotNull(challenge);
        }

        [Fact]
        public void WhenGenerateCalled_ThenChallengePropertiesNotEmpty()
        {
            var service = GetDefaultService();
            var challenge = service.Generate();

            Assert.False(string.IsNullOrEmpty(challenge.Challenge));
            Assert.False(string.IsNullOrEmpty(challenge.Algorithm));
            Assert.False(string.IsNullOrEmpty(challenge.Signature));
            Assert.False(string.IsNullOrEmpty(challenge.Salt));
        }

        [Fact]
        public void WhenGenerateCalledTwice_ThenRandomStringIsDifferent()
        {
            var service = GetDefaultService();
            var challenge1 = service.Generate();
            var challenge2 = service.Generate();

            var challenge1RandomString = challenge1.Salt.Split('?')[0];
            var challenge2RandomString = challenge2.Salt.Split('?')[0];

            Assert.NotEqual(challenge1RandomString, challenge2RandomString);
        }

        [Fact]
        public void WhenCreateCalled_ThenChallengeAlgorithmIsSha256()
        {
            var service = GetDefaultService();
            var challenge = service.Generate();

            Assert.Equal("SHA-256", challenge.Algorithm);
            Assert.False(string.IsNullOrEmpty(challenge.Algorithm));
        }

        [Fact]
        public void WhenCreateCalled_ThenSaltHasEndingDelimiter()
        {
            var service = GetDefaultService();
            var challenge = service.Generate();
            Assert.EndsWith("&", challenge.Salt);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(10, 20)]
        public void GivenCustomComplexity_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInRange(
            int min,
            int max)
        {
            var service = GetServiceWithComplexity(min, max);

            TestComplexityWithinRange(min, max, () => service.Generate());
        }

        [Theory]
        [InlineData(0, 1, OverrideMethod.Deprecated)]
        [InlineData(10, 20, OverrideMethod.Deprecated)]
        [InlineData(120, 130, OverrideMethod.Deprecated)]
        [InlineData(0, 1, OverrideMethod.ConfigurationRecord)]
        [InlineData(10, 20, OverrideMethod.ConfigurationRecord)]
        [InlineData(120, 130, OverrideMethod.ConfigurationRecord)]
        public void
            GivenCustomComplexityWithOverrides_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInRange(
                int min,
                int max,
                OverrideMethod overrideMethod)
        {
            const int initialMin = 100;
            const int initialMax = 110;

            var service = GetServiceWithComplexity(initialMin, initialMax);
            var overrides = new AltchaGenerateChallengeOverrides
            {
                Complexity = new AltchaComplexity(min, max)
            };

            TestComplexityWithinRange(min,
                                      max,
                                      () => GenerateWithOverride(service, overrides, overrideMethod));
        }

        [Fact]
        public void
            GivenCustomComplexity_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInclusiveRange()
        {
            const int min = 10;
            const int max = 12;

            var service = GetServiceWithComplexity(min, max);

            TestComplexityInclusiveRange(min, max, () => service.Generate());
        }

        [Theory]
        [InlineData(OverrideMethod.Deprecated)]
        [InlineData(OverrideMethod.ConfigurationRecord)]
        public void
            GivenCustomComplexityWithOverrides_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInclusiveRange(
                OverrideMethod overrideMethod)
        {
            const int initialMin = 100;
            const int initialMax = 102;

            const int min = 10;
            const int max = 12;

            var service = GetServiceWithComplexity(initialMin, initialMax);
            var overrides = new AltchaGenerateChallengeOverrides
            {
                Complexity = new AltchaComplexity(min, max)
            };

            TestComplexityInclusiveRange(min,
                                         max,
                                         () => GenerateWithOverride(service, overrides, overrideMethod));
        }

        [Theory]
        [InlineData(CommonServiceValidationMethod.Base64, OverrideMethod.Deprecated)]
        [InlineData(CommonServiceValidationMethod.Object, OverrideMethod.Deprecated)]
        [InlineData(CommonServiceValidationMethod.Base64, OverrideMethod.ConfigurationRecord)]
        [InlineData(CommonServiceValidationMethod.Object, OverrideMethod.ConfigurationRecord)]
        public async Task GivenChallengeIsSolvedAfterExpiryOverride_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceValidationMethod validationMethod,
            OverrideMethod overrideMethod)
        {
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.ChallengeExpired;
            const string expectedErrorString = "Challenge expired.";

            var service = GetServiceWithExpiry(30);
            var overrides = new AltchaGenerateChallengeOverrides
            {
                Expiry = AltchaExpiry.FromSeconds(5)
            };
            var challenge = GenerateWithOverride(service, overrides, overrideMethod);
            _clock.SetOffsetInSeconds(10);
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);
            var validationResult = await ValidateWithMethod(service, result.Altcha, validationMethod);

            Assert.True(result.Succeeded);
            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedErrorCode, validationResult.ValidationError.Code);
            Assert.Equal(expectedErrorString, validationResult.ValidationError.Message);
        }

        [Theory]
        [InlineData(CommonServiceValidationMethod.Base64, OverrideMethod.Deprecated)]
        [InlineData(CommonServiceValidationMethod.Object, OverrideMethod.Deprecated)]
        [InlineData(CommonServiceValidationMethod.Base64, OverrideMethod.ConfigurationRecord)]
        [InlineData(CommonServiceValidationMethod.Object, OverrideMethod.ConfigurationRecord)]
        public async Task
            GivenChallengeIsSolvedWithinExpiryOverride_WhenCallingValidate_ReturnsPositiveResult(
                CommonServiceValidationMethod validationMethod,
                OverrideMethod overrideMethod)
        {
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.NoError;
            const string expectedErrorString = "";

            var service = GetServiceWithExpiry(1);
            var overrides = new AltchaGenerateChallengeOverrides
            {
                Expiry = AltchaExpiry.FromSeconds(30)
            };
            var challenge = GenerateWithOverride(service, overrides, overrideMethod);
            _clock.SetOffsetInSeconds(10);
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);
            var validationResult = await ValidateWithMethod(service, result.Altcha, validationMethod);

            Assert.True(result.Succeeded);
            Assert.True(validationResult.IsValid);
            Assert.Equal(expectedErrorCode, validationResult.ValidationError.Code);
            Assert.Equal(expectedErrorString, validationResult.ValidationError.Message);
        }

        [Fact]
        public void GivenDeprecatedChallengeOverridesAreNull_WhenCallingGenerate_ThenThrowException()
        {
            var service = Altcha.CreateServiceBuilder()
                                .UseInMemoryStore()
                                .UseSha256(TestUtils.GetKey())
                                .Build();
            Assert.Throws<ArgumentNullException>(() =>
                                                     service.Generate((AltchaGenerateChallengeOverrides)
                                                                      null));
        }

        [Fact]
        public void GivenChallengeOverridesAreNull_WhenCallingGenerate_ThenThrowException()
        {
            var service = Altcha.CreateServiceBuilder()
                                .UseInMemoryStore()
                                .UseSha256(TestUtils.GetKey())
                                .Build();
            Assert.Throws<ArgumentNullException>(() =>
                                                     service.Generate((Func<AltchaSha256Configuration,
                                                                          AltchaSha256Configuration>)null));
        }

        [Fact]
        public void GivenChallengeOverridesReturnsNull_WhenCallingGenerate_ThenThrowException()
        {
            var service = Altcha.CreateServiceBuilder()
                                .UseInMemoryStore()
                                .UseSha256(TestUtils.GetKey())
                                .Build();
            Assert.Throws<ArgumentNullException>(() => service.Generate(_ => null));
        }

        [Theory]
        [InlineData(CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceValidationMethod.Object)]
        public async Task GivenChallengeOverridesIncludesKey_WhenCallingGenerate_UsesDifferentKey(
            CommonServiceValidationMethod validationMethod)
        {
            var store = new InMemoryStore(_clock);
            var key1 = TestUtils.GetKey();
            var key2 = TestUtils.GetKey();
            key2[0] = 2;
            var service1 = Altcha.CreateService(new AltchaSha256Configuration
            {
                StoreFactory = () => store,
                Key = AltchaKey.FromBytes(key1)
            });
            var service2 = Altcha.CreateService(new AltchaSha256Configuration
            {
                StoreFactory = () => store,
                Key = AltchaKey.FromBytes(key2)
            });

            var challenge = GenerateWithOverride(service1,
                                                 new AltchaGenerateChallengeOverrides(),
                                                 OverrideMethod.ConfigurationRecord,
                                                 AltchaKey.FromBytes(key2));

            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);

            var validationResult1 = await ValidateWithMethod(service1, result.Altcha, validationMethod);
            Assert.False(validationResult1.IsValid);

            var validationResult2 = await ValidateWithMethod(service2, result.Altcha, validationMethod);
            Assert.True(validationResult2.IsValid);
        }

        private static void TestComplexityWithinRange(int min,
                                                      int max,
                                                      Func<AltchaChallenge> generator)
        {
            for (var i = 0; i < 100; i++)
            {
                var challenge = generator();
                var simulation = new AltchaFrontEndSimulation();
                var result = simulation.Run(challenge);

                Assert.True(result.Succeeded);
                Assert.InRange(result.Number, min, max);
            }
        }

        private static void TestComplexityInclusiveRange(int min,
                                                         int max,
                                                         Func<AltchaChallenge> generator)
        {
            var minHit = false;
            var maxHit = false;

            for (var i = 0; i < 1000; i++)
            {
                var challenge = generator();
                var simulation = new AltchaFrontEndSimulation();
                var result = simulation.Run(challenge);

                Assert.False(result.Number < min || result.Number > max);
                if (result.Number == min)
                    minHit = true;

                if (result.Number == max)
                    maxHit = true;

                if (minHit && maxHit)
                    return;
            }

            Assert.Fail();
        }

        private static CommonService GetDefaultService()
        {
            return TestUtils.ServiceFactories[CommonServiceType.Default]
                            .GetDefaultService();
        }

        private static AltchaService GetServiceWithComplexity(int min, int max)
        {
            var key = TestUtils.GetKey();
            return Altcha.CreateServiceBuilder()
                         .UseSha256(key)
                         .SetComplexity(min, max)
                         .UseInMemoryStore()
                         .Build();
        }

        private AltchaService GetServiceWithExpiry(int expiry)
        {
            var key = TestUtils.GetKey();
            return Altcha.CreateServiceBuilder()
                         .UseClock(_clock)
                         .UseSha256(key)
                         .SetExpiryInSeconds(expiry)
                         .UseInMemoryStore()
                         .Build();
        }

        private AltchaChallenge GenerateWithOverride(AltchaService service,
                                                     AltchaGenerateChallengeOverrides overrides,
                                                     OverrideMethod overrideMethod,
                                                     AltchaKey altchaKeyOverride = null)
        {
            switch (overrideMethod)
            {
                case OverrideMethod.Deprecated:
                    return service.Generate(overrides);
                case OverrideMethod.ConfigurationRecord:
#if NET8_0_OR_GREATER
                    return service.Generate((configuration) => configuration with
                    {
                        Complexity = overrides.Complexity.HasValue
                                         ? configuration.Complexity with
                                         {
                                             Counter =
                                             new AltchaComplexityCounterRange(overrides.Complexity.Value.Min,
                                                 overrides.Complexity.Value.Max),
                                         }
                                         : configuration.Complexity,
                        Expiry = overrides.Expiry ?? configuration.Expiry,
                        Key = altchaKeyOverride ?? configuration.Key,
                    });
#else
                    return service.Generate(configuration =>
                    {
                        if (overrides.Complexity.HasValue)
                            configuration.Complexity.Counter =
                                new AltchaComplexityCounterRange(overrides.Complexity.Value.Min,
                                                                 overrides.Complexity.Value.Max);

                        if (overrides.Expiry.HasValue)
                            configuration.Expiry = overrides.Expiry.Value;

                        if (altchaKeyOverride != null)
                            configuration.Key = altchaKeyOverride;

                        return configuration;
                    });
#endif
                default:
                    throw new InvalidOperationException();
            }
        }

        private async static Task<AltchaValidationResult> ValidateWithMethod(
            AltchaService service,
            AltchaResponseSet response,
            CommonServiceValidationMethod validationMethod)
        {
            switch (validationMethod)
            {
                case CommonServiceValidationMethod.Base64:
                    return await service.Validate(response.Base64);
                case CommonServiceValidationMethod.Object:
                    return await service.Validate(response.Object);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
