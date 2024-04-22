using System.Threading.Tasks;

namespace Altcha.Net.Internal.Response
{
    internal interface IResponseValidator
    {
        Task<IAltchaValidationResult> Validate(string altchaBase64);
    }
}
