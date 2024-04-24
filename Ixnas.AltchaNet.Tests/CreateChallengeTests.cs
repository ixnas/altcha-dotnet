namespace Ixnas.AltchaNet.Tests;

public class CreateChallengeTests
{
    [Fact]
    public void WhenCreateCalled_ThenChallengeNotNull()
    {
        var service = GetDefaultService();
        var challenge = service.Generate();
        Assert.NotNull(challenge);
    }

    [Fact]
    public void WhenCreateCalled_ThenChallengePropertiesNotEmpty()
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

    [Fact]
    public async Task GivenChallengeIsSolved_WhenCallingValidate_ReturnsPositiveResult()
    {
        var service = GetServiceWithComplexity(10, 20);
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GivenChallengedIsSolvedAfterExpiry_WhenCallingValidate_ReturnsNegativeResult()
    {
        var service = GetServiceWithExpiry(1);
        var challenge = service.Generate();
        Thread.Sleep(1100);
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.False(validationResult.IsValid);
    }

    [Fact]
    public async Task GivenChallengeIsSolved_WhenCallingValidateTwice_ReturnsNegativeResult()
    {
        var service = GetServiceWithComplexity(10, 20);
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        await service.Validate(result.AltchaJson);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.False(validationResult.IsValid);
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

    [Fact]
    public async Task GivenMalformedSignature_WhenCallingValidate_ReturnsNegativeResult()
    {
        await TestMalformedSimulation(signature => "x" + signature.Substring(1), null);
    }

    [Fact]
    public async Task GivenMalformedShortSignature_WhenCallingValidate_ReturnsNegativeResult()
    {
        await TestMalformedSimulation(signature => signature.Substring(1), null);
    }

    [Fact]
    public async Task GivenMalformedChallenge_WhenCallingValidate_ReturnsNegativeResult()
    {
        await TestMalformedSimulation(null, challenge => "x" + challenge.Substring(1));
    }

    [Fact]
    public async Task GivenMalformedShortChallenge_WhenCallingValidate_ReturnsNegativeResult()
    {
        await TestMalformedSimulation(null, challenge => challenge.Substring(1));
    }

    private static async Task TestMalformedSimulation(Func<string, string>? malformSignatureFn,
                                                      Func<string, string>? malformChallengeFn)
    {
        var service = GetServiceWithComplexity(10, 20);
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge,
                                    malformSignatureFn,
                                    malformChallengeFn);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.False(validationResult.IsValid);
    }

    private static AltchaService GetDefaultService()
    {
        var key = TestUtils.GetKey();
        return Altcha.CreateServiceBuilder()
                     .UseSha256(key)
                     .UseInMemoryStore()
                     .Build();
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

    private static AltchaService GetServiceWithExpiry(int expiryInSeconds)
    {
        var key = TestUtils.GetKey();
        return Altcha.CreateServiceBuilder()
                     .UseSha256(key)
                     .SetComplexity(10, 20)
                     .SetExpiryInSeconds(expiryInSeconds)
                     .UseInMemoryStore()
                     .Build();
    }
}
