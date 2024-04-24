namespace Ixnas.AltchaNet.Internal.Converters
{
    internal interface IBytesStringConverter
    {
        string GetHexStringFromBytes(byte[] bytes);
        Result<byte[]> GetByteArrayFromHexString(string hexString);
        byte[] GetByteArrayFromUtf8String(string utf8String);
    }
}
