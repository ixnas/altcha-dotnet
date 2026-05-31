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

        [Fact]
        public void GivenFactoryMethodsCalled_ReturnsNewSolverInstance()
        {
            var builder1 = Altcha.CreateSolver();
            Assert.NotNull(builder1);

            var builder2 = Altcha.CreateSolver(new AltchaSolverConfiguration()
            {
                IgnoreExpiry = true,
            });
            Assert.NotNull(builder2);
        }
    }
}
