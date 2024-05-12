namespace Ixnas.AltchaNet.Internal.Common.Converters
{
    internal interface PayloadConverter
    {
        Result<byte[]> Convert(string payload);
    }
}
