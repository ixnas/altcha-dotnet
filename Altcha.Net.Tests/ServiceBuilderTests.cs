using Altcha.Net.Exceptions;

namespace Altcha.Net.Tests;

public class ServiceBuilderTests
{
    private readonly IAltchaServiceBuilder _builder = Altcha.CreateServiceBuilder();

    [Fact]
    public void GivenNoStoreSpecified_WhenBuildCalled_ThenThrowsMissingStoreException()
    {
        var key = TestUtils.GetKey();
        var builder = _builder.AddKey(key);
        Assert.Throws<MissingStoreException>(() => builder.Build());
    }

    [Fact]
    public void GivenNoKeySpecified_WhenBuildCalled_ThenThrowsMissingStoreException()
    {
        var builder = _builder.AddStore(new AltchaChallengeStoreFake());
        Assert.Throws<MissingKeyException>(() => builder.Build());
    }

    [Fact]
    public void GivenStoreIsNull_WhenAddStoreCalled_ThenThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.AddStore(null));
    }

    [Fact]
    public void GivenKeyIsNull_WhenAddKeyCalled_ThenThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _builder.AddKey(null));
    }

    [Fact]
    public void GivenKeyIsTooShort_WhenAddKeyCalled_ThenThrowsInvalidKeyException()
    {
        Assert.Throws<InvalidKeyException>(() => _builder.AddKey([0, 1, 2]));
    }

    [Fact]
    public void GivenKeyIs64Bytes_WhenAddKeyCalled_ThenReturnBuilder()
    {
        var key = TestUtils.GetKey();
        var builder = _builder.AddKey(key);
        Assert.NotNull(builder);
    }

    [Fact]
    public void GivenStoreIsNotNull_WhenAddStoreCalled_ThenReturnsBuilder()
    {
        var store = new AltchaChallengeStoreFake();
        var builder = _builder.AddStore(store);
        Assert.NotNull(builder);
    }

    [Fact]
    public void GivenStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
    {
        var key = TestUtils.GetKey();
        var store = new AltchaChallengeStoreFake();
        var service = _builder.AddStore(store)
                              .AddKey(key)
                              .Build();
        Assert.NotNull(service);
    }

    [Fact]
    public void GivenInMemoryStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
    {
        var key = TestUtils.GetKey();
        var service = _builder.AddInMemoryStore()
                              .AddKey(key)
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
        var builder = _builder.AddInMemoryStore()
                              .AddKey(key);

        Assert.Throws<InvalidComplexityException>(() => builder.SetComplexity(min, max));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 10)]
    [InlineData(10, 50)]
    public void GivenComplexityIsValid_WhenSetComplexityCalled_ThenReturnBuilder(int min, int max)
    {
        var key = TestUtils.GetKey();
        var builder = _builder.AddInMemoryStore()
                              .AddKey(key)
                              .SetComplexity(min, max);
        Assert.NotNull(builder);
    }
}
