using Ixnas.AltchaNet.Internal.Common.Converters;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Common
{
    internal class ApiPayloadConverter : PayloadConverter
    {
        public Result<byte[]> Convert(string payload)
        {
            return new Result<byte[]>
            {
                Success = true,
                Value = ByteConverter.GetByteArrayFromUtf8String(payload)
            };
        }
    }
}
