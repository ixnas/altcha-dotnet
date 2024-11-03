using Ixnas.AltchaNet.Internal.Common.Converters;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Common
{
    internal class ApiPayloadConverter : PayloadConverter
    {
        public Result<byte[]> Convert(string payload)
        {
            return Result<byte[]>.Ok(ByteConverter.GetByteArrayFromUtf8String(payload));
        }
    }
}
