using Ixnas.AltchaNet.Internal.Common.Converters;
using Ixnas.AltchaNet.Internal.Common.Cryptography;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class SignatureGenerator
    {
        private readonly CryptoAlgorithm _cryptoAlgorithm;
        private readonly PayloadConverter _payloadConverter;

        public SignatureGenerator(CryptoAlgorithm cryptoAlgorithm,
                                  PayloadConverter payloadConverter)
        {
            _cryptoAlgorithm = cryptoAlgorithm;
            _payloadConverter = payloadConverter;
        }

        public Signature Generate(string payload)
        {
            var payloadBytesResult = _payloadConverter.Convert(payload);
            var payloadBytes = payloadBytesResult.Value;
            var signatureBytes = _cryptoAlgorithm.Sign(payloadBytes);

            return new Signature(signatureBytes,
                                 _payloadConverter,
                                 _cryptoAlgorithm);
        }
    }
}
