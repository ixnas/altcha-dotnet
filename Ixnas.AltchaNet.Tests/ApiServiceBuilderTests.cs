using System;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Tests.Fakes;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class ApiServiceBuilderTests
    {
        private readonly AltchaApiServiceBuilder _builder;

        public ApiServiceBuilderTests()
        {
            _builder = Altcha.CreateApiServiceBuilder();
        }

        [Fact]
        public void GivenNoApiSecretWasSet_WhenBuildCalled_ThenThrowException()
        {
            var builder = _builder.UseInMemoryStore();
            Assert.Throws<MissingApiSecretException>(() => builder.Build());
        }

        [Fact]
        public void GivenNoStoreWasSet_WhenBuildCalled_ThenThrowException()
        {
            var builder = _builder.UseApiSecret("csec_8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f");
            Assert.Throws<MissingStoreException>(() => builder.Build());
        }

        [Fact]
        public void GivenStoreIsNull_WhenAddStoreCalled_ThenThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _builder.UseStore(null as IAltchaChallengeStore));
        }

        [Fact]
        public void GivenCancellableStoreIsNull_WhenAddStoreCalled_ThenThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                                                     _builder.UseStore(null as
                                                                           IAltchaCancellableChallengeStore));
        }

        [Fact]
        public void GivenStoreFactoryIsNull_WhenAddStoreCalled_ThenThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                                                     _builder.UseStore(null as Func<IAltchaChallengeStore>));
        }

        [Fact]
        public void GivenCancellableStoreFactoryIsNull_WhenAddStoreCalled_ThenThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                                                     _builder.UseStore(null as Func<
                                                                           IAltchaCancellableChallengeStore>));
        }

        [Fact]
        public void GivenStoreIsNotNull_WhenAddStoreCalled_ThenReturnsBuilder()
        {
            var store = new AltchaChallengeStoreFake();
            var builder = _builder.UseStore((IAltchaChallengeStore)store);
            Assert.NotNull(builder);
        }

        [Fact]
        public void GivenCancellableStoreIsNotNull_WhenAddStoreCalled_ThenReturnsBuilder()
        {
            var store = new AltchaChallengeStoreFake();
            var builder = _builder.UseStore((IAltchaCancellableChallengeStore)store);
            Assert.NotNull(builder);
        }

        [Fact]
        public void GivenStoreFactoryIsNotNull_WhenAddStoreCalled_ThenReturnsBuilder()
        {
            var store = new AltchaChallengeStoreFake();
            var builder = _builder.UseStore(() => (IAltchaChallengeStore)store);
            Assert.NotNull(builder);
        }

        [Fact]
        public void GivenCancellableStoreFactoryIsNotNull_WhenAddStoreCalled_ThenReturnsBuilder()
        {
            var store = new AltchaChallengeStoreFake();
            var builder = _builder.UseStore(() => (IAltchaCancellableChallengeStore)store);
            Assert.NotNull(builder);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.1)]
        public void
            GivenMaxSpamFilterScoreIsZeroOrNegative_WhenSetMaxSpamFilterScoreCalled_ThenThrowsException(
                double scoreThreshold)
        {
            Assert.Throws<InvalidMaxSpamFilterScoreException>(() =>
                                                                  _builder
                                                                      .SetMaxSpamFilterScore(scoreThreshold));
        }

        [Fact]
        public void GivenMaxSpamFilterScoreIsPositive_WhenSetMaxSpamFilterScoreCalled_ThenReturnsBuilder()
        {
            var builder = _builder.SetMaxSpamFilterScore(0.5);
            Assert.NotNull(builder);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("ec_8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f")]
        [InlineData("xrs_8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f")]
        [InlineData("8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f")]
        public void GivenApiSecretIsInvalid_WhenBuildCalled_ThenThrowException(string secret)
        {
            var builder = _builder.UseInMemoryStore();
            Assert.Throws<InvalidApiSecretException>(() => builder.UseApiSecret(secret));
        }

        [Theory]
        [InlineData("csec_8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f")]
        [InlineData("sec_8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f8f")]
        public void GivenApiSecretIsValid_WhenBuildCalled_ThenReturnBuilder(string secret)
        {
            var builder = _builder.UseApiSecret(secret);
            Assert.NotNull(builder);
        }

        [Fact]
        public void GivenStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var store = new AltchaChallengeStoreFake();
            var service = _builder.UseStore((IAltchaChallengeStore)store)
                                  .UseApiSecret(TestUtils.GetApiSecret())
                                  .Build();
            Assert.NotNull(service);
        }

        [Fact]
        public void GivenCancellableStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var store = new AltchaChallengeStoreFake();
            var service = _builder.UseStore((IAltchaCancellableChallengeStore)store)
                                  .UseApiSecret(TestUtils.GetApiSecret())
                                  .Build();
            Assert.NotNull(service);
        }

        [Fact]
        public void GivenStoreFactoryAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var store = new AltchaChallengeStoreFake();
            var service = _builder.UseStore(() => (IAltchaChallengeStore)store)
                                  .UseApiSecret(TestUtils.GetApiSecret())
                                  .Build();
            Assert.NotNull(service);
        }

        [Fact]
        public void GivenCancellableStoreFactoryAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var store = new AltchaChallengeStoreFake();
            var service = _builder.UseStore(() => (IAltchaCancellableChallengeStore)store)
                                  .UseApiSecret(TestUtils.GetApiSecret())
                                  .Build();
            Assert.NotNull(service);
        }

        [Fact]
        public void GivenInMemoryStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var service = _builder.UseInMemoryStore()
                                  .UseApiSecret(TestUtils.GetApiSecret())
                                  .Build();
            Assert.NotNull(service);
        }

        [Fact]
        public void WhenBuilderMethodsAreCalled_ThenReturnNewBuilderInstance()
        {
            var store = new AltchaChallengeStoreFake();
            var builder2 = _builder.UseStore((IAltchaChallengeStore)store);
            Assert.NotEqual(_builder, builder2);

            var builder3 = builder2.UseApiSecret(TestUtils.GetApiSecret());
            Assert.NotEqual(builder2, builder3);

            var builder4 = builder3.UseInMemoryStore();
            Assert.NotEqual(builder3, builder4);

            var builder5 = builder4.UseStore(() => (IAltchaChallengeStore)store);
            Assert.NotEqual(builder4, builder5);

            var builder6 = builder5.UseStore((IAltchaCancellableChallengeStore)store);
            Assert.NotEqual(builder5, builder6);

            var builder7 = builder6.UseStore(() => (IAltchaCancellableChallengeStore)store);
            Assert.NotEqual(builder6, builder7);
        }
    }
}
