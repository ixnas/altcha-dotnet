using Ixnas.AltchaNet.Debug;

namespace Ixnas.AltchaNet.Tests;

public class CreateChallengeTests
{
    private readonly ClockFake _clock = new();

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
        var service = GetServiceWithComplexity(1, 3);
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task GivenChallengedIsSolvedAfterExpiry_WhenCallingValidate_ReturnsNegativeResult()
    {
        var service = GetServiceWithExpiry(1, null, _clock);
        var challenge = service.Generate();
        _clock.SetOffsetInSeconds(2);
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.False(validationResult.IsValid);
    }

    [Fact]
    public async Task GivenChallengeIsSolvedWithOldService_WhenCallingValidateOnNewService_RespectsOldExpiry()
    {
        var service = GetServiceWithExpiry(1, null, _clock);
        var challenge = service.Generate();
        _clock.SetOffsetInSeconds(2);
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var newService = GetServiceWithExpiry(30, null, _clock);
        var validationResult = await newService.Validate(result.AltchaJson);
        Assert.False(validationResult.IsValid);
    }

    [Fact]
    public async Task GivenChallengedHasExpiry_WhenCallingValidate_StoresMatchingExpiry()
    {
        var store = new AltchaChallengeStoreFake();
        var service = GetServiceWithExpiry(30, store);
        var challenge = service.Generate();
        var tenSecondsFromNow = DateTimeOffset.UtcNow.AddSeconds(30);
        var marginStart = tenSecondsFromNow.AddSeconds(-2);
        var marginEnd = tenSecondsFromNow.AddSeconds(2);

        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        await service.Validate(result.AltchaJson);

        var expiry = store.Stored!.Value.Expiry;
        Assert.InRange(expiry, marginStart, marginEnd);
    }

    [Fact]
    public async Task GivenChallengeIsSolved_WhenCallingValidateTwice_ReturnsNegativeResult()
    {
        var service = GetServiceWithComplexity(1, 3);
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

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    public async Task GivenMalformedSignature_WhenCallingValidate_ReturnsNegativeResult(string prefix)
    {
        await TestMalformedSimulation(signature => prefix + signature.Substring(1), null);
    }

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    public async Task GivenMalformedChallenge_WhenCallingValidate_ReturnsNegativeResult(string prefix)
    {
        await TestMalformedSimulation(null, challenge => prefix + challenge.Substring(1));
    }

    [Fact]
    public async Task
        GivenStoredChallengesAreExpired_WhenChallengeIsValidated_CleansExpiredChallengesFromInMemoryStore()
    {
        var service = GetServiceWithExpiry(20, null, _clock);
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var run1 = await service.Validate(result.AltchaJson);
        Assert.True(run1.IsValid);

        _clock.SetOffsetInSeconds(40);
        var run2 = await service.Validate(result.AltchaJson); // cleaned
        Assert.False(run2.IsValid);

        _clock.SetOffsetInSeconds(0);
        var run3 = await service.Validate(result.AltchaJson);
        Assert.True(run3.IsValid);
    }

    private async Task TestMalformedSimulation(Func<string, string>? malformSignatureFn,
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

    private AltchaService GetDefaultService()
    {
        var key = TestUtils.GetKey();
        return Altcha.CreateServiceBuilder()
                     .UseSha256(key)
                     .UseInMemoryStore()
                     .Build();
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

    private AltchaService GetServiceWithExpiry(int expiryInSeconds,
                                               IAltchaChallengeStore? store = null,
                                               Clock? clock = null)
    {
        var key = TestUtils.GetKey();
        var builder = Altcha.CreateServiceBuilder()
                            .UseSha256(key)
                            .SetComplexity(1, 3)
                            .SetExpiryInSeconds(expiryInSeconds);

        if (store != null)
            builder = builder.UseStore(store);
        else
            builder = builder.UseInMemoryStore();

        if (clock != null)
            builder = builder.UseClock(clock);

        return builder.Build();
    }
}
