using System.Threading.Tasks;
using Altcha.Net.Internal.Challenge;
using Altcha.Net.Internal.Response;

namespace Altcha.Net.Internal
{
    internal class Service : IAltchaService
    {
        private readonly IChallengeGenerator _challengeGenerator;
        private readonly IResponseValidator _responseValidator;

        public Service(IChallengeGenerator challengeGenerator,
                       IResponseValidator responseValidator)
        {
            _challengeGenerator = challengeGenerator;
            _responseValidator = responseValidator;
        }

        public IAltchaChallenge GenerateChallenge()
        {
            return _challengeGenerator.Generate();
        }

        public async Task<IAltchaValidationResult> ValidateResponse(string altchaBase64)
        {
            return await _responseValidator.Validate(altchaBase64);
        }
    }
}
