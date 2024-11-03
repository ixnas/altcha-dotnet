using Ixnas.AltchaNet.Internal.Common.Converters;
using Ixnas.AltchaNet.Internal.Common.Cryptography;

namespace Ixnas.AltchaNet.Internal.SpamFilter
{
    internal class ApiSpamFilterPayloadConverter : PayloadConverter
    {
        private readonly CryptoAlgorithm _cryptoAlgorithm;

        public ApiSpamFilterPayloadConverter(CryptoAlgorithm cryptoAlgorithm)
        {
            _cryptoAlgorithm = cryptoAlgorithm;
        }

        public Result<byte[]> Convert(string payload)
        {
            if (payload == null)
                return Result<byte[]>.Fail();
            var challengeBytes = ByteConverter.GetByteArrayFromUtf8String(payload);
            var challengeHash = _cryptoAlgorithm.Hash(challengeBytes);
            return Result<byte[]>.Ok(challengeHash);
        }
    }
}
