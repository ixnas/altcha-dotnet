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

            TestComplexityWithinRange(min, max, service.Generate);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(10, 20)]
        public void
            GivenCustomComplexityWithOverrides_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInRange(
                int min,
                int max)
        {
            const int initialMin = 100;
            const int initialMax = 110;

            var service = GetServiceWithComplexity(initialMin, initialMax);

            TestComplexityWithinRange(min,
                                      max,
                                      () => service.Generate(config => config with
                                      {
                                          Complexity = config.Complexity with
                                          {
                                              Counter = new AltchaComplexityCounterRange(min, max)
                                          }
                                      }));
        }

        [Fact]
        public void
            GivenCustomComplexity_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInclusiveRange()
        {
            const int min = 10;
            const int max = 12;

            var service = GetServiceWithComplexity(min, max);

            TestComplexityInclusiveRange(min, max, service.Generate);
        }

        [Fact]
        public void
            GivenCustomComplexityWithOverrides_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInclusiveRange()
        {
            const int initialMin = 100;
            const int initialMax = 102;

            const int min = 10;
            const int max = 12;

            var service = GetServiceWithComplexity(initialMin, initialMax);

            TestComplexityWithinRange(min,
                                      max,
                                      () => service.Generate(config => config with
                                      {
                                          Complexity = config.Complexity with
                                          {
                                              Counter = new AltchaComplexityCounterRange(min, max)
                                          }
                                      }));
        }

        [Theory]
        [InlineData(CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceValidationMethod.Object)]
        public async Task GivenChallengeIsSolvedAfterExpiryOverride_WhenCallingValidate_ReturnsNegativeResult(
            CommonServiceValidationMethod validationMethod)
        {
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.ChallengeExpired;
            const string expectedErrorString = "Challenge expired.";

            var service = GetServiceWithExpiry(30);
            var challenge = service.Generate(config => config with
            {
                Expiry = AltchaExpiry.FromSeconds(5)
            });
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
        [InlineData(CommonServiceValidationMethod.Base64)]
        [InlineData(CommonServiceValidationMethod.Object)]
        public async Task
            GivenChallengeIsSolvedWithinExpiryOverride_WhenCallingValidate_ReturnsPositiveResult(
                CommonServiceValidationMethod validationMethod)
        {
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.NoError;
            const string expectedErrorString = "";

            var service = GetServiceWithExpiry(1);
            var challenge = service.Generate(config => config with
            {
                Expiry = AltchaExpiry.FromSeconds(30)
            });
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
        public void GivenChallengeOverridesAreNull_WhenCallingGenerate_ThenThrowException()
        {
            var inMemoryStore = new InMemoryStore(_clock);
            var service = Altcha.CreateService(new AltchaSha256Configuration()
            {
                StoreFactory = () => inMemoryStore,
                Key = AltchaKey.FromBytes(TestUtils.GetKey()),
            });
            Assert.Throws<ArgumentNullException>(() => service.Generate(null));
        }

        [Fact]
        public void GivenChallengeOverridesReturnsNull_WhenCallingGenerate_ThenThrowException()
        {
            var inMemoryStore = new InMemoryStore(_clock);
            var service = Altcha.CreateService(new AltchaSha256Configuration()
            {
                StoreFactory = () => inMemoryStore,
                Key = AltchaKey.FromBytes(TestUtils.GetKey()),
            });
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

            var challenge = service1.Generate(config => config with
            {
                Key = AltchaKey.FromBytes(key2)
            });

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

        private AltchaService GetServiceWithComplexity(int min, int max)
        {
            var inMemoryStore = new InMemoryStore(_clock);
            return Altcha.CreateService(new AltchaSha256Configuration()
            {
                StoreFactory = () => inMemoryStore,
                Key = AltchaKey.FromBytes(TestUtils.GetKey()),
                Complexity = new AltchaDeterministicComplexity()
                {
                    Cost = 1,
                    Counter = new AltchaComplexityCounterRange(min, max)
                }
            });
        }

        private AltchaService GetServiceWithExpiry(int expiry)
        {
            var inMemoryStore = new InMemoryStore(_clock);
            return Altcha.CreateService(new AltchaSha256Configuration()
            {
                StoreFactory = () => inMemoryStore,
                Key = AltchaKey.FromBytes(TestUtils.GetKey()),
                Expiry = AltchaExpiry.FromSeconds(expiry)
            }, _clock);
        }

        private static async Task<AltchaValidationResult> ValidateWithMethod(
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
