namespace Ixnas.AltchaNet.Internal.Serialization
{
    internal interface JsonSerializer
    {
        T FromBase64Json<T>(string base64);
        string ToBase64Json<T>(T obj);
    }
}
