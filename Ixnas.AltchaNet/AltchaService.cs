using System.Threading;
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
            return _challengeGenerator.Generate(new AltchaGenerateChallengeOverrides());
        }

        /// <summary>
        ///     Generates a new ALTCHA challenge.
        /// </summary>
        /// <param name="overrides">Configuration overrides applied to only this generation.</param>
        /// <returns></returns>
        public AltchaChallenge Generate(AltchaGenerateChallengeOverrides overrides)
        {
            return _challengeGenerator.Generate(overrides);
        }

        /// <summary>
        ///     Validates a solved ALTCHA challenge response.
        /// </summary>
        /// <param name="altchaBase64">A base64-encoded ALTCHA response, typically a form field named "altcha".</param>
        /// <returns>A result object representing the result of the validation.</returns>
        public async Task<AltchaValidationResult> Validate(string altchaBase64)
        {
            return await _responseValidator.Validate(altchaBase64, CancellationToken.None);
        }

        /// <summary>
        ///     Validates a solved ALTCHA challenge response.
        /// </summary>
        /// <param name="altchaBase64">A base64-encoded ALTCHA response, typically a form field named "altcha".</param>
        /// <param name="cancellationToken">A cancellation token to cancel I/O related work.</param>
        /// <returns>A result object representing the result of the validation.</returns>
        public async Task<AltchaValidationResult> Validate(string altchaBase64,
                                                           CancellationToken cancellationToken)
        {
            return await _responseValidator.Validate(altchaBase64, cancellationToken);
        }

        /// <summary>
        ///     Validates a solved ALTCHA challenge response.
        /// </summary>
        /// <param name="altchaResponse">A (deserialized) ALTCHA response.</param>
        /// <returns>A result object representing the result of the validation.</returns>
        public async Task<AltchaValidationResult> Validate(AltchaResponse altchaResponse)
        {
            return await _responseValidator.Validate(altchaResponse, CancellationToken.None);
        }

        /// <summary>
        ///     Validates a solved ALTCHA challenge response.
        /// </summary>
        /// <param name="altchaResponse">A (deserialized) ALTCHA response.</param>
        /// <param name="cancellationToken">A cancellation token to cancel I/O related work.</param>
        /// <returns>A result object representing the result of the validation.</returns>
        public async Task<AltchaValidationResult> Validate(AltchaResponse altchaResponse,
                                                           CancellationToken cancellationToken)
        {
            return await _responseValidator.Validate(altchaResponse, cancellationToken);
        }
    }
}
