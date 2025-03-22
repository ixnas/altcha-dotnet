using Ixnas.AltchaNet.Exceptions;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class AltchaExpiryTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void GivenExpiryIsInvalid_WhenSetExpiryInSecondsCalled_ThenThrowException(int expiryInSeconds)
        {
            Assert.Throws<InvalidExpiryException>(() => AltchaExpiry.FromSeconds(expiryInSeconds));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        public void GivenExpiryIsValid_WhenConstructingObject_ThenPropertyMatchesInput(
            int expiryInSeconds)
        {
            var expiry = AltchaExpiry.FromSeconds(expiryInSeconds);

            Assert.Equal(expiryInSeconds, expiry.Seconds);
        }
    }
}
