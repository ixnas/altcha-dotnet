using System;
using Altcha.Net.Exceptions;
using Altcha.Net.Internal.Challenge;
using Altcha.Net.Internal.Cryptography;
using Altcha.Net.Internal.Response;
using Altcha.Net.Internal.Serialization;

namespace Altcha.Net.Internal
{
    internal class ServiceBuilder : IAltchaServiceBuilder
    {
        private readonly byte[] _key;
        private readonly int _max = Constants.DefaultComplexityMax;
        private readonly int _min = Constants.DefaultComplexityMin;
        private readonly IAltchaChallengeStore _store;

        public ServiceBuilder()
        {
        }

        private ServiceBuilder(IAltchaChallengeStore store,
                               byte[] key,
                               int min,
                               int max)
        {
            _store = store;
            _key = key;
            _min = min;
            _max = max;
        }

        public IAltchaService Build()
        {
            if (_store == null)
                throw new MissingStoreException();
            if (_key == null)
                throw new MissingKeyException();

            var saltGenerator = new GuidSaltGenerator();
            var randomNumberGenerator = new BasicRandomNumberGenerator();
            var crypto = new Sha256Crypto(_key);
            var serializer = new SystemTextJsonSerializer();

            var challengeGenerator = new ChallengeGenerator(saltGenerator,
                                                            randomNumberGenerator,
                                                            crypto,
                                                            _min,
                                                            _max);
            var responseValidator = new ResponseValidator(_store, crypto, serializer);

            return new Service(challengeGenerator, responseValidator);
        }

        public IAltchaServiceBuilder AddStore(IAltchaChallengeStore store)
        {
            if (store == null)
                throw new ArgumentNullException();
            return new ServiceBuilder(store, _key, _min, _max);
        }

        public IAltchaServiceBuilder AddKey(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length != Constants.RequiredKeySize)
                throw new InvalidKeyException();
            return new ServiceBuilder(_store, key, _min, _max);
        }

        public IAltchaServiceBuilder AddInMemoryStore()
        {
            var store = new InMemoryStore();
            return new ServiceBuilder(store, _key, _min, _max);
        }

        public IAltchaServiceBuilder SetComplexity(int min, int max)
        {
            if (min < 0 || max < 0 || min > max)
                throw new InvalidComplexityException();
            return new ServiceBuilder(_store, _key, min, max);
        }
    }
}
