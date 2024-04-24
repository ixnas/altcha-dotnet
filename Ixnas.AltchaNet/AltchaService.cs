using System.Threading.Tasks;
using Ixnas.AltchaNet.Internal.Challenge;
using Ixnas.AltchaNet.Internal.Response;

namespace Ixnas.AltchaNet
{
    public sealed class AltchaService : IChallengeGenerator, IResponseValidator
    {
        private readonly IChallengeGenerator _challengeGenerator;
        private readonly IResponseValidator _responseValidator;

        internal AltchaService(IChallengeGenerator challengeGenerator,
                               IResponseValidator responseValidator)
        {
            _challengeGenerator = challengeGenerator;
            _responseValidator = responseValidator;
        }

        /// <summary>
        ///     Generates a new ALTCHA challenge.
        /// </summary>
        public AltchaChallenge Generate()
        {
            return _challengeGenerator.Generate();
        }

        /// <summary>
        ///     Validates a solved ALTCHA challenge response.
        /// </summary>
        /// <param name="altchaBase64">A base64-encoded ALTCHA response, typically a form field named "altcha".</param>
        /// <returns>A result object representing the result of the validation.</returns>
        public async Task<AltchaValidationResult> Validate(string altchaBase64)
        {
            return await _responseValidator.Validate(altchaBase64);
        }
    }
}
