using System;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Tests.Fakes;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class ServiceBuilderTests
    {
        public enum SettingParameter
        {
            Primitives,
            Struct
        }

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
            var builder = _builder.UseStore((IAltchaCancellableChallengeStore)new AltchaChallengeStoreFake());
            Assert.Throws<MissingAlgorithmException>(() => builder.Build());
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
        public void GivenKeyIsNull_WhenUseSha256Called_ThenThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _builder.UseSha256(null));
        }

        [Fact]
        public void GivenKeyIsTooShort_WhenUseSha256Called_ThenThrowsInvalidKeyException()
        {
            Assert.Throws<InvalidKeyException>(() => _builder.UseSha256(new byte[] { 0, 1, 2 }));
        }

        [Fact]
        public void GivenKeyIs64Bytes_WhenUseSha256Called_ThenReturnBuilder()
        {
            var key = TestUtils.GetKey();
            var builder = _builder.UseSha256(key);
            Assert.NotNull(builder);
        }

        [Fact]
        public void GivenStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var key = TestUtils.GetKey();
            var store = new AltchaChallengeStoreFake();
            var service = _builder.UseStore((IAltchaChallengeStore)store)
                                  .UseSha256(key)
                                  .Build();
            Assert.NotNull(service);
        }

        [Fact]
        public void GivenStoreFactoryAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var key = TestUtils.GetKey();
            var store = new AltchaChallengeStoreFake();
            var service = _builder.UseStore(() => (IAltchaChallengeStore)store)
                                  .UseSha256(key)
                                  .Build();
            Assert.NotNull(service);
        }

        [Fact]
        public void GivenCancellableStoreAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var key = TestUtils.GetKey();
            var store = new AltchaChallengeStoreFake();
            var service = _builder.UseStore((IAltchaCancellableChallengeStore)store)
                                  .UseSha256(key)
                                  .Build();
            Assert.NotNull(service);
        }

        [Fact]
        public void GivenCancellableStoreFactoryAndKeyAreAdded_WhenBuildCalled_ThenReturnNewService()
        {
            var key = TestUtils.GetKey();
            var store = new AltchaChallengeStoreFake();
            var service = _builder.UseStore(() => (IAltchaCancellableChallengeStore)store)
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
        [InlineData(-10, 10, SettingParameter.Primitives)]
        [InlineData(10, -10, SettingParameter.Primitives)]
        [InlineData(10, 5, SettingParameter.Primitives)]
        [InlineData(-10, 10, SettingParameter.Struct)]
        [InlineData(10, -10, SettingParameter.Struct)]
        [InlineData(10, 5, SettingParameter.Struct)]
        public void GivenMinimumAndMaximumAreInvalid_WhenSetComplexityCalled_ThenThrowException(
            int min,
            int max,
            SettingParameter parameter)
        {
            var key = TestUtils.GetKey();
            var builder = _builder.UseInMemoryStore()
                                  .UseSha256(key);

            Assert.Throws<InvalidComplexityException>(() => SetComplexityWithParameter(builder,
                                                               min,
                                                               max,
                                                               parameter));
        }

        [Theory]
        [InlineData(0, 0, SettingParameter.Primitives)]
        [InlineData(10, 10, SettingParameter.Primitives)]
        [InlineData(10, 50, SettingParameter.Primitives)]
        [InlineData(0, 0, SettingParameter.Struct)]
        [InlineData(10, 10, SettingParameter.Struct)]
        [InlineData(10, 50, SettingParameter.Struct)]
        public void GivenComplexityIsValid_WhenSetComplexityCalled_ThenReturnBuilder(
            int min,
            int max,
            SettingParameter parameter)
        {
            var key = TestUtils.GetKey();
            var builder = _builder.UseInMemoryStore()
                                  .UseSha256(key);
            builder = SetComplexityWithParameter(builder,
                                                 min,
                                                 max,
                                                 parameter);
            Assert.NotNull(builder);
        }

        [Theory]
        [InlineData(0, SettingParameter.Primitives)]
        [InlineData(-10, SettingParameter.Primitives)]
        [InlineData(0, SettingParameter.Struct)]
        [InlineData(-10, SettingParameter.Struct)]
        public void GivenExpiryIsInvalid_WhenSetExpiryInSecondsCalled_ThenThrowException(
            int expiryInSeconds,
            SettingParameter parameter)
        {
            Assert.Throws<InvalidExpiryException>(() => SetExpiryWithParameter(Altcha.CreateServiceBuilder(),
                                                           expiryInSeconds,
                                                           parameter));
        }

        [Theory]
        [InlineData(1, SettingParameter.Primitives)]
        [InlineData(50, SettingParameter.Primitives)]
        [InlineData(1, SettingParameter.Struct)]
        [InlineData(50, SettingParameter.Struct)]
        public void GivenExpiryIsValid_WhenSetExpiryInSecondsCalled_ThenReturnBuilder(
            int expiryInSeconds,
            SettingParameter parameter)
        {
            var builder = SetExpiryWithParameter(Altcha.CreateServiceBuilder(), expiryInSeconds, parameter);
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
            var builder2 = _builder.UseStore((IAltchaCancellableChallengeStore)store);
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

            var builder7 = builder6.UseStore(() => (IAltchaChallengeStore)store);
            Assert.NotEqual(builder6, builder7);

            var builder8 = builder7.UseStore((IAltchaCancellableChallengeStore)store);
            Assert.NotEqual(builder7, builder8);

            var builder9 = builder8.UseStore(() => (IAltchaCancellableChallengeStore)store);
            Assert.NotEqual(builder8, builder9);
        }

        private static AltchaServiceBuilder SetComplexityWithParameter(
            AltchaServiceBuilder builder,
            int min,
            int max,
            SettingParameter parameter)
        {
            switch (parameter)
            {
                case SettingParameter.Primitives:
                    return builder.SetComplexity(min, max);
                case SettingParameter.Struct:
                    return builder.SetComplexity(new AltchaComplexity(min, max));
                default:
                    throw new InvalidOperationException();
            }
        }

        private static AltchaServiceBuilder SetExpiryWithParameter(
            AltchaServiceBuilder builder,
            int expiryInSeconds,
            SettingParameter parameter)
        {
            switch (parameter)
            {
                case SettingParameter.Primitives:
                    return builder.SetExpiryInSeconds(expiryInSeconds);
                case SettingParameter.Struct:
                    return builder.SetExpiry(AltchaExpiry.FromSeconds(expiryInSeconds));
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
