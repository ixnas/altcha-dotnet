using System.Linq;
using Ixnas.AltchaNet.Internal.Common.Converters;

namespace Ixnas.AltchaNet.Internal.Common.Cryptography
{
    internal class Signature
    {
        private readonly CryptoAlgorithm _cryptoAlgorithm;
        private readonly PayloadConverter _payloadConverter;
        private readonly byte[] _signatureBytes;

        public Signature(byte[] signatureBytes,
                         PayloadConverter payloadConverter,
                         CryptoAlgorithm cryptoAlgorithm)
        {
            _signatureBytes = signatureBytes;
            _payloadConverter = payloadConverter;
            _cryptoAlgorithm = cryptoAlgorithm;
        }

        public Result PayloadIsValid(string payload)
        {
            var error = Result.Fail(ErrorCode.PayloadDoesNotMatchSignature);

            var payloadBytesResult = _payloadConverter.Convert(payload);
            if (!payloadBytesResult.Success)
                return error;

            var payloadBytes = payloadBytesResult.Value;
            var calculatedSignature = _cryptoAlgorithm.Sign(payloadBytes);

            if (!_signatureBytes.SequenceEqual(calculatedSignature))
                return error;

            return Result.Ok();
        }

        public string ToHexString()
        {
            return ByteConverter.GetHexStringFromBytes(_signatureBytes);
        }
    }
}
