namespace Altcha.Net.Tests;

public class CreateChallengeTests
{
    [Fact]
    public void WhenCreateCalled_ThenChallengeNotNull()
    {
        var service = GetDefaultService();
        var challenge = service.GenerateChallenge();
        Assert.NotNull(challenge);
    }

    [Fact]
    public void WhenCreateCalled_ThenChallengePropertiesNotEmpty()
    {
        var service = GetDefaultService();
        var challenge = service.GenerateChallenge();

        Assert.False(string.IsNullOrEmpty(challenge.Challenge));
        Assert.False(string.IsNullOrEmpty(challenge.Algorithm));
        Assert.False(string.IsNullOrEmpty(challenge.Signature));
        Assert.False(string.IsNullOrEmpty(challenge.Salt));
    }

    [Fact]
    public async Task GivenChallengeIsSolved_WhenCallingValidate_ReturnsPositiveResult()
    {
        var service = GetServiceWithComplexity(10, 20);
        var challenge = service.GenerateChallenge();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var validationResult = await service.ValidateResponse(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task GivenChallengeIsSolved_WhenCallingValidateTwice_ReturnsNegativeResult()
    {
        var service = GetServiceWithComplexity(10, 20);
        var challenge = service.GenerateChallenge();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        await service.ValidateResponse(result.AltchaJson);
        var validationResult = await service.ValidateResponse(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.False(validationResult.IsValid);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 20)]
    [InlineData(50, 100)]
    [InlineData(250, 500)]
    public void GivenCustomComplexity_WhenCallingValidateMultipleTimes_ReturnsResultWithNumberInRange(
        int min,
        int max)
    {
        for (var i = 0; i < 100; i++)
        {
            var service = GetServiceWithComplexity(min, max);
            var challenge = service.GenerateChallenge();
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

        for (var i = 0; i < 1000; i++)
        {
            var service = GetServiceWithComplexity(min, max);
            var challenge = service.GenerateChallenge();
            var simulation = new AltchaFrontEndSimulation();
            var result = simulation.Run(challenge);

            if (result.Number < min || result.Number > max)
                Assert.Fail();

            if (result.Number == min)
                minHit = true;

            if (result.Number == max)
                maxHit = true;
        }

        if (minHit && maxHit)
            return;

        Assert.Fail();
    }

    private static IAltchaService GetDefaultService()
    {
        var key = TestUtils.GetKey();
        return Altcha.CreateServiceBuilder()
                     .AddKey(key)
                     .AddInMemoryStore()
                     .Build();
    }

    private static IAltchaService GetServiceWithComplexity(int min, int max)
    {
        var key = TestUtils.GetKey();
        return Altcha.CreateServiceBuilder()
                     .AddKey(key)
                     .SetComplexity(min, max)
                     .AddInMemoryStore()
                     .Build();
    }
}
