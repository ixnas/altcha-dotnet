using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Tests.Fakes;

namespace Ixnas.AltchaNet.Tests;

public class ServiceBuilderTests
{
    private readonly AltchaServiceBuilder _builder = Altcha.CreateServiceBuilder();

    [Fact]
    public void GivenNoStoreSpecified_WhenBuildCalled_ThenThrowsMissingStoreException()
    {
        var key = TestUtils.GetKey();
        var builder = _builder.UseSha256(key);
        Assert.Throws<MissingStoreException>(() => builder.Build());
    }

    [Fact]
    public void GivenNoKeySpecified_WhenBuildCalled_ThenThrowsMissingStoreException()
    {
        var builder = _builder.UseStore(new AltchaChallengeStoreFake());
        Assert.Throws<MissingAlgorithmException>(() => builder.Build());
    }

    [Fact]
    public void GivenStoreIsNull_WhenAddStoreCalled_ThenThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.UseStore(null));
    }

    [Fact]
    public void GivenKeyIsNull_WhenUseSha256Called_ThenThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.UseSha256(null));
    }

    [Fact]
    public void GivenKeyIsTooShort_WhenUseSha256Called_ThenThrowsInvalidKeyException()
    {
        Assert.Throws<InvalidKeyException>(() => _builder.UseSha256([0, 1, 2]));
    }

    [Fact]
    public void GivenKeyIs64Bytes_WhenUseSha256Called_ThenReturnBuilder()
    {
        var key = TestUtils.GetKey();
        var builder = _builder.UseSha256(key);
        Assert.NotNull(builder);
    }

    [Fact]
    public void GivenStoreIsNotNull_WhenAddStoreCalled_ThenReturnsBuilder()
    {
        var store = new AltchaChallengeStoreFake();
        var builder = _builder.UseStore(store);
        Assert.NotNull(builder);
    }

    [Fact]
    public void GivenStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
    {
        var key = TestUtils.GetKey();
        var store = new AltchaChallengeStoreFake();
        var service = _builder.UseStore(store)
                              .UseSha256(key)
                              .Build();
        Assert.NotNull(service);
    }

    [Fact]
    public void GivenInMemoryStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
    {
        var key = TestUtils.GetKey();
        var service = _builder.UseInMemoryStore()
                              .UseSha256(key)
                              .Build();
        Assert.NotNull(service);
    }

    [Theory]
    [InlineData(-10, 10)]
    [InlineData(10, -10)]
    [InlineData(10, 5)]
    public void GivenMinimumAndMaximumAreInvalid_WhenSetComplexityCalled_ThenThrowException(int min, int max)
    {
        var key = TestUtils.GetKey();
        var builder = _builder.UseInMemoryStore()
                              .UseSha256(key);

        Assert.Throws<InvalidComplexityException>(() => builder.SetComplexity(min, max));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 10)]
    [InlineData(10, 50)]
    public void GivenComplexityIsValid_WhenSetComplexityCalled_ThenReturnBuilder(int min, int max)
    {
        var key = TestUtils.GetKey();
        var builder = _builder.UseInMemoryStore()
                              .UseSha256(key)
                              .SetComplexity(min, max);
        Assert.NotNull(builder);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void GivenExpiryIsInvalid_WhenSetExpiryInSecondsCalled_ThenThrowException(int expiryInSeconds)
    {
        Assert.Throws<InvalidExpiryException>(() => _builder.SetExpiryInSeconds(expiryInSeconds));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void GivenExpiryIsValid_WhenSetExpiryInSecondsCalled_ThenReturnBuilder(int expiryInSeconds)
    {
        var builder = _builder.SetExpiryInSeconds(expiryInSeconds);
        Assert.NotNull(builder);
    }

    [Fact]
    public void GivenClockIsNull_WhenUseClockCalled_ThenThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.UseClock(null));
    }

    [Fact]
    public void GivenClockIsValid_WhenUseClockCalled_ThenReturnBuilder()
    {
        var clock = new ClockFake();
        var builder = _builder.UseClock(clock);
        Assert.NotNull(builder);
    }

    [Fact]
    public void WhenBuilderMethodsAreCalled_ThenReturnNewBuilderInstance()
    {
        var store = new AltchaChallengeStoreFake();
        var builder2 = _builder.UseStore(store);
        Assert.NotEqual(_builder, builder2);

        var key = TestUtils.GetKey();
        var builder3 = builder2.UseSha256(key);
        Assert.NotEqual(builder2, builder3);

        var builder4 = builder3.UseInMemoryStore();
        Assert.NotEqual(builder3, builder4);

        var builder5 = builder4.SetComplexity(1, 3);
        Assert.NotEqual(builder4, builder5);

        var builder6 = builder5.SetExpiryInSeconds(5);
        Assert.NotEqual(builder5, builder6);
    }
}
