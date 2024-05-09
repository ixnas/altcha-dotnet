using Ixnas.AltchaNet.Internal.Converters;

namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal class DefaultChallengeStringToBytesConverter : ChallengeStringToBytesConverter
    {
        private readonly BytesStringConverter _bytesStringConverter;

        public DefaultChallengeStringToBytesConverter(BytesStringConverter bytesStringConverter)
        {
            _bytesStringConverter = bytesStringConverter;
        }

        public Result<byte[]> Generate(string challenge)
        {
            return _bytesStringConverter.GetByteArrayFromHexString(challenge);
        }
    }
}
