using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class SolverBuilderTests
    {
        [Fact]
        public void GivenBuilderMethodsCalled_ReturnsNewBuilderInstance()
        {
            var builder = Altcha.CreateSolverBuilder();
            var builder2 = builder.IgnoreExpiry();
            Assert.NotEqual(builder, builder2);
        }
    }
}
