using Ixnas.AltchaNet.Exceptions;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class AltchaComplexityTests
    {
        [Theory]
        [InlineData(-10, 10)]
        [InlineData(10, -10)]
        [InlineData(10, 5)]
        public void GivenMinimumAndMaximumAreInvalid_WhenConstructingObject_ThenThrowException(
            int min,
            int max)
        {
            Assert.Throws<InvalidComplexityException>(() => new AltchaComplexity(min, max));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 10)]
        [InlineData(10, 50)]
        public void GivenMinimumAndMaximumAreValid_WhenConstructingObject_ThenPropertiesMatchInput(
            int min,
            int max)
        {
            var complexity = new AltchaComplexity(min, max);

            Assert.Equal(min, complexity.Min);
            Assert.Equal(max, complexity.Max);
        }
    }
}
