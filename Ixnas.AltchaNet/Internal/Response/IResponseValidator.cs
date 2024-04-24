using System.Threading.Tasks;

namespace Ixnas.AltchaNet.Internal.Response
{
    internal interface IResponseValidator
    {
        Task<AltchaValidationResult> Validate(string altchaBase64);
    }
}
