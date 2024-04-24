namespace Ixnas.AltchaNet.Internal.Serialization
{
    internal interface IJsonSerializer
    {
        T FromBase64Json<T>(string base64);
        string ToBase64Json<T>(T obj);
    }
}
