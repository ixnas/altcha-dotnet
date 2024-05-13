namespace Ixnas.AltchaNet.Internal.Common.Serialization
{
    internal interface JsonSerializer
    {
        Result<T> FromBase64Json<T>(string base64);
    }
}
