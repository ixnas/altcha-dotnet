namespace Altcha.Net.Internal.Serialization
{
    internal interface IJsonSerializer
    {
        T FromBase64Json<T>(string base64);
    }
}
