using System.Threading.Tasks;
using Ixnas.AltchaNet.Internal.ProofOfWork;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Service used for self-hosted ALTCHA challenges.
    /// </summary>
    public sealed class AltchaService
    {
        private readonly ChallengeGenerator _challengeGenerator;
        private readonly ResponseValidator _responseValidator;

        internal AltchaService(ChallengeGenerator challengeGenerator,
                               ResponseValidator responseValidator)
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
