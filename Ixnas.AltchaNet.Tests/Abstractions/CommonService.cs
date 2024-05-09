using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Tests.Simulations;

namespace Ixnas.AltchaNet.Tests.Abstractions;

public enum CommonServiceType
{
    Default,
    Api
}

internal interface CommonServiceFactory
{
    CommonService GetDefaultService();

    CommonService GetServiceWithExpiry(int expiryInSeconds,
                                       IAltchaChallengeStore? store = null,
                                       Clock? clock = null);
}

internal interface CommonService
{
    AltchaChallenge Generate();
    Task<AltchaValidationResult> Validate(string altcha64);
}

internal class CommonDefaultServiceFactory : CommonServiceFactory
{
    public CommonService GetDefaultService()
    {
        var key = TestUtils.GetKey();
        var service = Altcha.CreateServiceBuilder()
                            .SetComplexity(1, 3)
                            .UseSha256(key)
                            .UseInMemoryStore()
                            .Build();
        return new CommonDefaultService(service);
    }

    public CommonService GetServiceWithExpiry(int expiryInSeconds,
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

        var service = builder.Build();
        return new CommonDefaultService(service);
    }
}

internal class CommonApiServiceFactory : CommonServiceFactory
{
    public CommonService GetDefaultService()
    {
        var simulation = new AltchaApiSimulation(TestUtils.GetApiSecret());
        var service = Altcha.CreateApiServiceBuilder()
                            .UseInMemoryStore()
                            .UseApiSecret(TestUtils.GetApiSecret())
                            .Build();
        return new CommonApiService(service, simulation);
    }

    public CommonService GetServiceWithExpiry(int expiryInSeconds,
                                              IAltchaChallengeStore? store = null,
                                              Clock? clock = null)
    {
        var simulation = new AltchaApiSimulation(TestUtils.GetApiSecret());
        var builder = Altcha.CreateApiServiceBuilder()
                            .UseInMemoryStore()
                            .UseApiSecret(TestUtils.GetApiSecret());

        if (store != null)
            builder = builder.UseStore(store);
        else
            builder = builder.UseInMemoryStore();

        if (clock != null)
            builder = builder.UseClock(clock);

        var service = builder.Build();
        return new CommonApiService(service, simulation, expiryInSeconds);
    }
}

internal class CommonApiService : CommonService
{
    private readonly int _expiryInSeconds;
    private readonly AltchaApiService _service;
    private readonly AltchaApiSimulation _simulation;

    public CommonApiService(AltchaApiService service,
                            AltchaApiSimulation simulation,
                            int expiryInSeconds = 120)
    {
        _service = service;
        _simulation = simulation;
        _expiryInSeconds = expiryInSeconds;
    }

    public AltchaChallenge Generate()
    {
        return _simulation.Generate(_expiryInSeconds);
    }

    public Task<AltchaValidationResult> Validate(string altcha64)
    {
        return _service.Validate(altcha64);
    }
}

internal class CommonDefaultService : CommonService
{
    private readonly AltchaService _service;

    public CommonDefaultService(AltchaService service)
    {
        _service = service;
    }

    public AltchaChallenge Generate()
    {
        return _service.Generate();
    }

    public Task<AltchaValidationResult> Validate(string altcha64)
    {
        return _service.Validate(altcha64);
    }
}
