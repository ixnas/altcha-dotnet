using System;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Tests.Fakes;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class ServiceBuilderTests
    {
        // TODO Test storeFactory == null
        [Fact]
        public void GivenKeyIsNull_WhenKeyIsConstructed_ThenThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => AltchaKey.FromBytes(null));
        }

        [Fact]
        public void GivenKeyIsTooShort_WhenKeyIsConstructed_ThenThrowsInvalidKeyException()
        {
            Assert.Throws<InvalidKeyException>(() => AltchaKey.FromBytes([0, 1, 2]));
        }

        [Theory]
        [InlineData(-10, 10)]
        [InlineData(10, -10)]
        [InlineData(10, 5)]
        public void GivenMinimumAndMaximumAreInvalid_WhenBuiltWithComplexity_ThenThrowException(
            int min,
            int max)
        {
            var key = TestUtils.GetKey();

            Assert.Throws<InvalidComplexityException>(() => Altcha.CreateService(new AltchaSha256Configuration
            {
                StoreFactory = () => new InMemoryStore(new ClockFake()),
                Key = AltchaKey.FromBytes(key),
                Complexity = new AltchaDeterministicComplexity
                {
                    Counter = new AltchaComplexityCounterRange(min, max),
                    Cost = 1
                }
            }));
        }

        [Theory]
        [InlineData(-10, 10)]
        [InlineData(10, -10)]
        [InlineData(10, 5)]
        public void
            GivenMinimumAndMaximumAreInvalid_WhenDeterministicComplexityIsConstructed_ThenThrowException(
                int min,
                int max)
        {
            Assert.Throws<InvalidComplexityException>(() => new AltchaComplexityCounterRange(min, max));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 10)]
        [InlineData(10, 50)]
        public void GivenComplexityIsValid_WhenBuiltWithComplexity_ThenReturnService(
            int min,
            int max)
        {
            var key = TestUtils.GetKey();

            var service = Altcha.CreateService(new AltchaSha256Configuration
            {
                StoreFactory = () => new InMemoryStore(new ClockFake()),
                Key = AltchaKey.FromBytes(key),
                Complexity = new AltchaDeterministicComplexity
                {
                    Counter = new AltchaComplexityCounterRange(min, max),
                    Cost = 1
                }
            });

            Assert.NotNull(service);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void GivenExpiryIsInvalid_WhenExpiryIsConstructed_ThenThrowException(
            int expiryInSeconds)
        {
            Assert.Throws<InvalidExpiryException>(() => AltchaExpiry.FromSeconds(expiryInSeconds));
        }

    }
}
