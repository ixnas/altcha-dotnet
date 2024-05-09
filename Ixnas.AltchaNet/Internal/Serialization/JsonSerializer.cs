namespace Ixnas.AltchaNet.Internal.Serialization
{
    internal interface JsonSerializer
    {
        Result<T> FromBase64Json<T>(string base64);
        string ToBase64Json<T>(T obj);
    }
}
