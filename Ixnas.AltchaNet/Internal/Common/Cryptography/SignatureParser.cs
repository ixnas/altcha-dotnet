using Ixnas.AltchaNet.Internal.Common.Converters;

namespace Ixnas.AltchaNet.Internal.Common.Cryptography
{
    internal class SignatureParser
    {
        private readonly CryptoAlgorithm _cryptoAlgorithm;
        private readonly PayloadConverter _payloadConverter;

        public SignatureParser(PayloadConverter payloadConverter,
                               CryptoAlgorithm cryptoAlgorithm)
        {
            _payloadConverter = payloadConverter;
            _cryptoAlgorithm = cryptoAlgorithm;
        }

        public bool TryParse(string hexString, out Signature signature)
        {
            var result = Parse(hexString);
            if (!result.Success)
            {
                signature = null;
                return false;
            }

            signature = result.Value;
            return true;
        }

        private Result<Signature> Parse(string hexString)
        {
            if (hexString == null)
                return new Result<Signature>();
            var signatureBytesResult = ByteConverter.GetByteArrayFromHexString(hexString);
            if (!signatureBytesResult.Success)
                return new Result<Signature>();
            return new Result<Signature>
            {
                Success = true,
                Value = new Signature(signatureBytesResult.Value,
                                      _payloadConverter,
                                      _cryptoAlgorithm)
            };
        }
    }
}
