using Ixnas.AltchaNet.Tests.Abstractions;
using Ixnas.AltchaNet.Tests.Simulations;

namespace Ixnas.AltchaNet.Tests;

public class SelfHostedChallengeTests
{
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
    public void WhenCreateCalled_ThenChallengeAlgorithmIsSha256()
    {
        var service = GetDefaultService();
        var challenge = service.Generate();

        Assert.Equal("SHA-256", challenge.Algorithm);
        Assert.False(string.IsNullOrEmpty(challenge.Algorithm));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 20)]
    public void GivenCustomComplexity_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInRange(
        int min,
        int max)
    {
        var service = GetServiceWithComplexity(min, max);
        for (var i = 0; i < 100; i++)
        {
            var challenge = service.Generate();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);

            Assert.True(result.Succeeded);
            Assert.InRange(result.Number, min, max);
        }
    }

    [Fact]
    public void GivenCustomComplexity_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInclusiveRange()
    {
        var min = 10;
        var max = 12;

        var minHit = false;
        var maxHit = false;

        var service = GetServiceWithComplexity(min, max);
        for (var i = 0; i < 1000; i++)
        {
            var challenge = service.Generate();
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

    private CommonService GetDefaultService()
    {
        return TestUtils.ServiceFactories[CommonServiceType.Default]
                        .GetDefaultService();
    }

    private AltchaService GetServiceWithComplexity(int min, int max)
    {
        var key = TestUtils.GetKey();
        return Altcha.CreateServiceBuilder()
                     .UseSha256(key)
                     .SetComplexity(min, max)
                     .UseInMemoryStore()
                     .Build();
    }
}
