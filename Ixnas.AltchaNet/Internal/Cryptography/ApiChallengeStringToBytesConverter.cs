using Ixnas.AltchaNet.Internal.Converters;

namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal class ApiChallengeStringToBytesConverter : ChallengeStringToBytesConverter
    {
        private readonly BytesStringConverter _bytesStringConverter;

        public ApiChallengeStringToBytesConverter(BytesStringConverter bytesStringConverter)
        {
            _bytesStringConverter = bytesStringConverter;
        }

        public Result<byte[]> Generate(string challenge)
        {
            return new Result<byte[]>
            {
                Success = true,
                Value = _bytesStringConverter.GetByteArrayFromUtf8String(challenge)
            };
        }
    }
}
