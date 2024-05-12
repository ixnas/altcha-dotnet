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

        public bool PayloadIsValid(string payload)
        {
            var payloadBytesResult = _payloadConverter.Convert(payload);
            if (!payloadBytesResult.Success)
                return false;

            var payloadBytes = payloadBytesResult.Value;
            var calculatedSignature = _cryptoAlgorithm.Sign(payloadBytes);

            return _signatureBytes.SequenceEqual(calculatedSignature);
        }

        public string ToHexString()
        {
            return ByteConverter.GetHexStringFromBytes(_signatureBytes);
        }
    }
}
