using Ixnas.AltchaNet.Internal.Common.Converters;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Common
{
    internal class SelfHostedPayloadConverter : PayloadConverter
    {
        public Result<byte[]> Convert(string payload)
        {
            return ByteConverter.GetByteArrayFromHexString(payload);
        }
    }
}
