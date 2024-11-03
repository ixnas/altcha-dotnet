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

        public Result<Signature> Parse(string hexString)
        {
            if (hexString == null)
                return Result<Signature>.Fail(ErrorCode.SignatureIsInvalidHexString);
            var signatureBytesResult = ByteConverter.GetByteArrayFromHexString(hexString);
            if (!signatureBytesResult.Success)
                return Result<Signature>.Fail(ErrorCode.SignatureIsInvalidHexString);
            return Result<Signature>.Ok(new Signature(signatureBytesResult.Value,
                                                      _payloadConverter,
                                                      _cryptoAlgorithm));
        }
    }
}
