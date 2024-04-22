using System.Threading.Tasks;

namespace Altcha.Net
{
    public interface IAltchaService
    {
        IAltchaChallenge GenerateChallenge();
        Task<IAltchaValidationResult> ValidateResponse(string altchaBase64);
    }
}
