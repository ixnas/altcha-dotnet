using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class SolverBuilderTests
    {
        [Fact]
        public void GivenFactoryMethodsCalled_ReturnsNewSolverInstance()
        {
            var builder1 = Altcha.CreateSolver();
            Assert.NotNull(builder1);

            var builder2 = Altcha.CreateSolver(new AltchaSolverConfiguration
            {
                IgnoreExpiry = true
            });
            Assert.NotNull(builder2);
        }
    }
}
