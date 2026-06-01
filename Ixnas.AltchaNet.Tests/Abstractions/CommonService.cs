using System;
using System.Threading;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Debug;
using Ixnas.AltchaNet.Tests.Fakes;
using Ixnas.AltchaNet.Tests.Simulations;

namespace Ixnas.AltchaNet.Tests.Abstractions
{
    public enum CommonServiceType
    {
        Default,
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

        CommonService GetServiceWithStoreFactory(Func<IAltchaChallengeStore> storeFactory);
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
            var store = new InMemoryStore(new ClockFake());
            var key = TestUtils.GetKey();
            var service = Altcha.CreateService(new AltchaSha256Configuration
            {
                Key = AltchaKey.FromBytes(key),
                StoreFactory = () => store,
                Complexity = new AltchaDeterministicComplexity
                {
                    Counter = new AltchaComplexityCounterRange(1, 3),
                    Cost = 1
                }
            });
            return new CommonDefaultService(service);
        }

        public CommonService GetServiceWithExpiry(int expiryInSeconds,
                                                  IAltchaChallengeStore store = null,
                                                  Clock clock = null)
        {
            var key = TestUtils.GetKey();
            clock ??= new ClockFake();
            store ??= new InMemoryStore(clock);
            var config = new AltchaSha256Configuration
            {
                Key = AltchaKey.FromBytes(key),
                StoreFactory = () => store,
                Expiry = AltchaExpiry.FromSeconds(expiryInSeconds),
                Complexity = new AltchaDeterministicComplexity
                {
                    Counter = new AltchaComplexityCounterRange(1, 3),
                    Cost = 1
                }
            };
            var service = Altcha.CreateService(config, clock);
            return new CommonDefaultService(service);
        }

        public CommonService GetServiceWithStoreFactory(Func<IAltchaChallengeStore> storeFactory)
        {
            var key = TestUtils.GetKey();
            var service = Altcha.CreateService(new AltchaSha256Configuration
            {
                Key = AltchaKey.FromBytes(key),
                StoreFactory = storeFactory,
                Complexity = new AltchaDeterministicComplexity
                {
                    Counter = new AltchaComplexityCounterRange(1, 3),
                    Cost = 1
                }
            });
            return new CommonDefaultService(service);
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
