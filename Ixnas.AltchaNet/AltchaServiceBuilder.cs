using System;
using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Internal;
using Ixnas.AltchaNet.Internal.Challenge;
using Ixnas.AltchaNet.Internal.Converters;
using Ixnas.AltchaNet.Internal.Cryptography;
using Ixnas.AltchaNet.Internal.Response;
using Ixnas.AltchaNet.Internal.Salt;
using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet
{
    public sealed class AltchaServiceBuilder
    {
        private readonly int _expiryInSeconds = Defaults.ExpiryInSeconds;
        private readonly byte[] _key;
        private readonly int _max = Defaults.ComplexityMax;
        private readonly int _min = Defaults.ComplexityMin;
        private readonly IAltchaChallengeStore _store;

        internal AltchaServiceBuilder()
        {
        }

        private AltchaServiceBuilder(IAltchaChallengeStore store,
                                     byte[] key,
                                     int min,
                                     int max,
                                     int expiryInSeconds)
        {
            _store = store;
            _key = key;
            _min = min;
            _max = max;
            _expiryInSeconds = expiryInSeconds;
        }

        /// <summary>
        ///     Returns a new configured service instance.
        /// </summary>
        public AltchaService Build()
        {
            if (_store == null)
                throw new MissingStoreException();
            if (_key == null)
                throw new MissingAlgorithmException();

            var serializer = new SystemTextJsonSerializer();
            var randomNumberGenerator = new BasicRandomNumberGenerator();
            var cryptoAlgorithm = new Sha256CryptoAlgorithm(_key);
            var bytesStringConverter = new BytesStringConverter();
            var saltGenerator = new TimestampedSaltGenerator(serializer, _expiryInSeconds);
            var saltParser = new TimestampedSaltParser(serializer);

            var challengeGenerator = new ChallengeGenerator(saltGenerator,
                                                            randomNumberGenerator,
                                                            bytesStringConverter,
                                                            cryptoAlgorithm,
                                                            _min,
                                                            _max);
            var responseValidator = new ResponseValidator(_store,
                                                          serializer,
                                                          saltParser,
                                                          bytesStringConverter,
                                                          cryptoAlgorithm);

            return new AltchaService(challengeGenerator, responseValidator);
        }

        /// <summary>
        ///     (Required) Configures a store to use for previously verified ALTCHA responses. Used to prevent replay attacks.
        /// </summary>
        /// <param name="store">Store to use.</param>
        /// <returns>A new instance of the builder with the updated configuration.</returns>
        public AltchaServiceBuilder UseStore(IAltchaChallengeStore store)
        {
            if (store == null)
                throw new ArgumentNullException();
            return new AltchaServiceBuilder(store,
                                            _key,
                                            _min,
                                            _max,
                                            _expiryInSeconds);
        }

        /// <summary>
        ///     (Required) Configures the SHA-256 algorithm for hashing and signing. Currently the only supported algorithm.
        /// </summary>
        /// <param name="key">A byte array representing the key to use. Must be exactly 64 bytes long.</param>
        /// <returns>A new instance of the builder with the updated configuration.</returns>
        public AltchaServiceBuilder UseSha256(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException();
            if (key.Length != Defaults.RequiredKeySize)
                throw new InvalidKeyException();
            return new AltchaServiceBuilder(_store,
                                            key,
                                            _min,
                                            _max,
                                            _expiryInSeconds);
        }

        /// <summary>
        ///     Configures a simple in-memory store for previously verified ALTCHA responses. Should only be used for testing
        ///     purposes.
        /// </summary>
        /// <returns>A new instance of the builder with the updated configuration.</returns>
        public AltchaServiceBuilder UseInMemoryStore()
        {
            var store = new InMemoryStore();
            return new AltchaServiceBuilder(store,
                                            _key,
                                            _min,
                                            _max,
                                            _expiryInSeconds);
        }

        /// <summary>
        ///     (Optional) Overrides the default complexity to tweak the amount of computational effort a client has to put in. See
        ///     https://altcha.org/docs/complexity/ for more information
        /// </summary>
        /// <param name="min">Minimum complexity (default 50,000)</param>
        /// <param name="max">Maximum complexity (default 100,000)</param>
        /// <returns>A new instance of the builder with the updated configuration.</returns>
        public AltchaServiceBuilder SetComplexity(int min, int max)
        {
            if (min < 0 || max < 0 || min > max)
                throw new InvalidComplexityException();
            return new AltchaServiceBuilder(_store,
                                            _key,
                                            min,
                                            max,
                                            _expiryInSeconds);
        }

        /// <summary>
        ///     (Optional) Overrides the default time it takes for a challenge to expire.
        /// </summary>
        /// <param name="expiryInSeconds">Expiry in seconds (default 120)</param>
        /// <returns>A new instance of the builder with the updated configuration.</returns>
        public AltchaServiceBuilder SetExpiryInSeconds(int expiryInSeconds)
        {
            if (expiryInSeconds < 1)
                throw new InvalidExpiryException();
            return new AltchaServiceBuilder(_store,
                                            _key,
                                            _min,
                                            _max,
                                            expiryInSeconds);
        }
    }
}
