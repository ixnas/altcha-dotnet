using Ixnas.AltchaNet.Tests.Abstractions;
using Ixnas.AltchaNet.Tests.Fakes;
using Ixnas.AltchaNet.Tests.Simulations;

namespace Ixnas.AltchaNet.Tests;

public class CommonChallengeTests
{
    private readonly ClockFake _clock = new();

    [Theory]
    [InlineData(null, CommonServiceType.Default)]
    [InlineData("", CommonServiceType.Default)]
    [InlineData(" ", CommonServiceType.Default)]
    [InlineData(null, CommonServiceType.Api)]
    [InlineData("", CommonServiceType.Api)]
    [InlineData(" ", CommonServiceType.Api)]
    public async Task GivenChallengeIsEmpty_WhenValidateCalled_ThenThrowException(
        string altcha,
        CommonServiceType commonServiceType)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetDefaultService();
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.Validate(altcha));
    }

    [Theory]
    [InlineData(CommonServiceType.Default)]
    [InlineData(CommonServiceType.Api)]
    public async Task GivenChallengeIsSolved_WhenCallingValidate_ReturnsPositiveResult(
        CommonServiceType commonServiceType)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetDefaultService();
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.True(validationResult.IsValid);
    }

    [Theory]
    [InlineData(CommonServiceType.Default)]
    [InlineData(CommonServiceType.Api)]
    public async Task GivenChallengedIsSolvedAfterExpiry_WhenCallingValidate_ReturnsNegativeResult(
        CommonServiceType commonServiceType)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetServiceWithExpiry(1, null, _clock);
        var challenge = service.Generate();
        _clock.SetOffsetInSeconds(2);
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.False(validationResult.IsValid);
    }

    [Theory]
    [InlineData(CommonServiceType.Default)]
    [InlineData(CommonServiceType.Api)]
    public async Task GivenChallengeIsSolvedWithOldService_WhenCallingValidateOnNewService_RespectsOldExpiry(
        CommonServiceType commonServiceType)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetServiceWithExpiry(1, null, _clock);
        var challenge = service.Generate();
        _clock.SetOffsetInSeconds(2);
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        var newService = TestUtils.ServiceFactories[commonServiceType]
                                  .GetServiceWithExpiry(30, null, _clock);
        var validationResult = await newService.Validate(result.AltchaJson);
        Assert.False(validationResult.IsValid);
    }

    [Theory]
    [InlineData(CommonServiceType.Default)]
    [InlineData(CommonServiceType.Api)]
    public async Task GivenChallengedHasExpiry_WhenCallingValidate_StoresMatchingExpiry(
        CommonServiceType commonServiceType)
    {
        var store = new AltchaChallengeStoreFake();
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetServiceWithExpiry(30, store);
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

    [Theory]
    [InlineData(CommonServiceType.Default)]
    [InlineData(CommonServiceType.Api)]
    public async Task GivenChallengeIsSolved_WhenCallingValidateTwice_ReturnsNegativeResult(
        CommonServiceType commonServiceType)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetDefaultService();
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge);
        await service.Validate(result.AltchaJson);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.False(validationResult.IsValid);
    }

    [Theory]
    [InlineData("", CommonServiceType.Api)]
    [InlineData("", CommonServiceType.Default)]
    [InlineData("x", CommonServiceType.Api)]
    [InlineData("x", CommonServiceType.Default)]
    public async Task GivenMalformedSignature_WhenCallingValidate_ReturnsNegativeResult(
        string prefix,
        CommonServiceType commonServiceType)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetDefaultService();
        await TestMalformedSimulation(service, signature => prefix + signature[1..], null);
    }

    [Theory]
    [InlineData("", CommonServiceType.Api)]
    [InlineData("", CommonServiceType.Default)]
    [InlineData("x", CommonServiceType.Api)]
    [InlineData("x", CommonServiceType.Default)]
    public async Task GivenMalformedChallenge_WhenCallingValidate_ReturnsNegativeResult(
        string prefix,
        CommonServiceType commonServiceType)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetDefaultService();
        await TestMalformedSimulation(service, null, challenge => prefix + challenge[1..]);
    }

    [Theory]
    [InlineData(CommonServiceType.Default, "")]
    [InlineData(CommonServiceType.Default, null)]
    [InlineData(CommonServiceType.Default, "weirojoij")]
    [InlineData(CommonServiceType.Default, "eyJzb21ldGhpbmciOiJlbHNlIiwiaXNudCI6InJpZ2h0In0=")]
    [InlineData(CommonServiceType.Api, "")]
    [InlineData(CommonServiceType.Api, null)]
    [InlineData(CommonServiceType.Api, "weirojoij")]
    [InlineData(CommonServiceType.Api, "iowjeroij.jwojeorij")]
    public async Task GivenMalformedSalt_WhenCallingValidate_ReturnsNegativeResult(
        CommonServiceType commonServiceType,
        string malformedSalt)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetDefaultService();
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge,
                                    null,
                                    null,
                                    _ => malformedSalt);
        var run = await service.Validate(result.AltchaJson);

        Assert.False(run.IsValid);
    }

    [Theory]
    [InlineData(CommonServiceType.Default, "weirojoij")]
    [InlineData(CommonServiceType.Default, "eyJzb21ldGhpbmciOiJlbHNlIiwiaXNudCI6InJpZ2h0In0=")]
    [InlineData(CommonServiceType.Api, "weirojoij")]
    [InlineData(CommonServiceType.Api, "eyJzb21ldGhpbmciOiJlbHNlIiwiaXNudCI6InJpZ2h0In0=")]
    public async Task GivenMalformedAltchaBase64_WhenCallingValidate_ReturnsNegativeResult(
        CommonServiceType commonServiceType,
        string malformedAltcha64)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetDefaultService();
        var run = await service.Validate(malformedAltcha64);
        Assert.False(run.IsValid);
    }

    [Theory]
    [InlineData(CommonServiceType.Default)]
    [InlineData(CommonServiceType.Api)]
    public async Task
        GivenStoredChallengesAreExpired_WhenChallengeIsValidated_CleansExpiredChallengesFromInMemoryStore(
            CommonServiceType commonServiceType)
    {
        var service = TestUtils.ServiceFactories[commonServiceType]
                               .GetServiceWithExpiry(20, null, _clock);
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

    private async static Task TestMalformedSimulation(CommonService service,
                                                      Func<string, string>? malformSignatureFn,
                                                      Func<string, string>? malformChallengeFn)
    {
        var challenge = service.Generate();
        var simulation = new AltchaFrontEndSimulation();
        var result = simulation.Run(challenge,
                                    malformSignatureFn,
                                    malformChallengeFn);
        var validationResult = await service.Validate(result.AltchaJson);

        Assert.True(result.Succeeded);
        Assert.False(validationResult.IsValid);
    }
}
