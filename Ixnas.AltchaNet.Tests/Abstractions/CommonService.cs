using System;
using System.Threading;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Tests.Simulations;

namespace Ixnas.AltchaNet.Tests.Abstractions
{
    public enum CommonServiceType
    {
        Default,
        Api
    }

    public enum CommonServiceValidationMethod
    {
        Base64,
        Object
    }

    internal interface CommonServiceFactory
    {
        CommonService GetDefaultService();

        CommonService GetServiceWithExpiry(int expiryInSeconds,
                                           IAltchaChallengeStore store = null,
                                           Clock clock = null);

        CommonService GetServiceWithExpiry(int expiryInSeconds,
                                           IAltchaCancellableChallengeStore store = null,
                                           Clock clock = null);

        CommonService GetServiceWithStoreFactory(Func<IAltchaChallengeStore> storeFactory);
        CommonService GetServiceWithStoreFactory(Func<IAltchaCancellableChallengeStore> storeFactory);
    }

    internal interface CommonService
    {
        AltchaChallenge Generate();

        Task<AltchaValidationResult> Validate(AltchaResponseSet altcha,
                                              CommonServiceValidationMethod commonServiceValidationMethod);

        Task<AltchaValidationResult> Validate(AltchaResponseSet altcha,
                                              CommonServiceValidationMethod commonServiceValidationMethod,
                                              CancellationToken cancellationToken);
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
                                                  IAltchaChallengeStore store = null,
                                                  Clock clock = null)
        {
            var key = TestUtils.GetKey();
            var builder = Altcha.CreateServiceBuilder()
                                .UseSha256(key)
                                .SetComplexity(1, 3)
                                .SetExpiryInSeconds(expiryInSeconds);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (store != null)
                builder = builder.UseStore(store);
            else
                builder = builder.UseInMemoryStore();

            if (clock != null)
                builder = builder.UseClock(clock);

            var service = builder.Build();
            return new CommonDefaultService(service);
        }

        public CommonService GetServiceWithExpiry(int expiryInSeconds,
                                                  IAltchaCancellableChallengeStore store = null,
                                                  Clock clock = null)
        {
            var key = TestUtils.GetKey();
            var builder = Altcha.CreateServiceBuilder()
                                .UseSha256(key)
                                .SetComplexity(1, 3)
                                .SetExpiryInSeconds(expiryInSeconds);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (store != null)
                builder = builder.UseStore(store);
            else
                builder = builder.UseInMemoryStore();

            if (clock != null)
                builder = builder.UseClock(clock);

            var service = builder.Build();
            return new CommonDefaultService(service);
        }

        public CommonService GetServiceWithStoreFactory(Func<IAltchaChallengeStore> storeFactory)
        {
            var key = TestUtils.GetKey();
            var service = Altcha.CreateServiceBuilder()
                                .SetComplexity(1, 3)
                                .UseSha256(key)
                                .UseStore(storeFactory)
                                .Build();
            return new CommonDefaultService(service);
        }

        public CommonService GetServiceWithStoreFactory(Func<IAltchaCancellableChallengeStore> storeFactory)
        {
            var key = TestUtils.GetKey();
            var service = Altcha.CreateServiceBuilder()
                                .SetComplexity(1, 3)
                                .UseSha256(key)
                                .UseStore(storeFactory)
                                .Build();
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
                                                  IAltchaChallengeStore store = null,
                                                  Clock clock = null)
        {
            var simulation = new AltchaApiSimulation(TestUtils.GetApiSecret());
            var builder = Altcha.CreateApiServiceBuilder()
                                .UseInMemoryStore()
                                .UseApiSecret(TestUtils.GetApiSecret());

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (store != null)
                builder = builder.UseStore(store);
            else
                builder = builder.UseInMemoryStore();

            if (clock != null)
                builder = builder.UseClock(clock);

            var service = builder.Build();
            return new CommonApiService(service, simulation, expiryInSeconds);
        }

        public CommonService GetServiceWithExpiry(int expiryInSeconds,
                                                  IAltchaCancellableChallengeStore store = null,
                                                  Clock clock = null)
        {
            var simulation = new AltchaApiSimulation(TestUtils.GetApiSecret());
            var builder = Altcha.CreateApiServiceBuilder()
                                .UseInMemoryStore()
                                .UseApiSecret(TestUtils.GetApiSecret());

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (store != null)
                builder = builder.UseStore(store);
            else
                builder = builder.UseInMemoryStore();

            if (clock != null)
                builder = builder.UseClock(clock);

            var service = builder.Build();
            return new CommonApiService(service, simulation, expiryInSeconds);
        }

        public CommonService GetServiceWithStoreFactory(Func<IAltchaChallengeStore> storeFactory)
        {
            var simulation = new AltchaApiSimulation(TestUtils.GetApiSecret());
            var service = Altcha.CreateApiServiceBuilder()
                                .UseStore(storeFactory)
                                .UseApiSecret(TestUtils.GetApiSecret())
                                .Build();
            return new CommonApiService(service, simulation);
        }

        public CommonService GetServiceWithStoreFactory(Func<IAltchaCancellableChallengeStore> storeFactory)
        {
            var simulation = new AltchaApiSimulation(TestUtils.GetApiSecret());
            var service = Altcha.CreateApiServiceBuilder()
                                .UseStore(storeFactory)
                                .UseApiSecret(TestUtils.GetApiSecret())
                                .Build();
            return new CommonApiService(service, simulation);
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

        public Task<AltchaValidationResult> Validate(AltchaResponseSet altcha,
                                                     CommonServiceValidationMethod
                                                         commonServiceValidationMethod)
        {
            switch (commonServiceValidationMethod)
            {
                case CommonServiceValidationMethod.Base64:
                    return _service.Validate(altcha.Base64);
                case CommonServiceValidationMethod.Object:
                    return _service.Validate(altcha.Object);
                default:
                    throw new InvalidOperationException();
            }
        }

        public Task<AltchaValidationResult> Validate(AltchaResponseSet altcha,
                                                     CommonServiceValidationMethod
                                                         commonServiceValidationMethod,
                                                     CancellationToken cancellationToken)
        {
            switch (commonServiceValidationMethod)
            {
                case CommonServiceValidationMethod.Base64:
                    return _service.Validate(altcha.Base64, cancellationToken);
                case CommonServiceValidationMethod.Object:
                    return _service.Validate(altcha.Object, cancellationToken);
                default:
                    throw new InvalidOperationException();
            }
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

        public Task<AltchaValidationResult> Validate(AltchaResponseSet altcha,
                                                     CommonServiceValidationMethod
                                                         commonServiceValidationMethod)
        {
            switch (commonServiceValidationMethod)
            {
                case CommonServiceValidationMethod.Base64:
                    return _service.Validate(altcha.Base64);
                case CommonServiceValidationMethod.Object:
                    return _service.Validate(altcha.Object);
                default:
                    throw new InvalidOperationException();
            }
        }

        public Task<AltchaValidationResult> Validate(AltchaResponseSet altcha,
                                                     CommonServiceValidationMethod
                                                         commonServiceValidationMethod,
                                                     CancellationToken cancellationToken)
        {
            switch (commonServiceValidationMethod)
            {
                case CommonServiceValidationMethod.Base64:
                    return _service.Validate(altcha.Base64, cancellationToken);
                case CommonServiceValidationMethod.Object:
                    return _service.Validate(altcha.Object, cancellationToken);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
