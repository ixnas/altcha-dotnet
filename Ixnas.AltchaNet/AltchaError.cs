namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Regular challenge validation error.
    /// </summary>
    public sealed class AltchaValidationError
    {
        /// <summary>
        ///     Human-readable representation of the error.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        ///     Programmatic representation of the error.
        /// </summary>
        public AltchaValidationErrorCode Code { get; set; }
    }

    /// <summary>
    ///     Spam filtered challenge validation error. Is not used to determine if a form has passed through the spam filter.
    /// </summary>
    public sealed class AltchaSpamFilteredValidationError
    {
        /// <summary>
        ///     Human-readable representation of the error.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        ///     Programmatic representation of the error.
        /// </summary>
        public AltchaSpamFilteredValidationErrorCode Code { get; set; }
    }

    /// <summary>
    ///     Solver error.
    /// </summary>
    public sealed class AltchaSolverError
    {
        /// <summary>
        ///     Human-readable representation of the error.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        ///     Programmatic representation of the error.
        /// </summary>
        public AltchaSolverErrorCode Code { get; set; }
    }

    /// <summary>
    ///     Possible error codes for invalid regular challenges.
    /// </summary>
    public enum AltchaValidationErrorCode
    {
        /// <summary>
        ///     Default error code for when validation succeeded.
        /// </summary>
        NoError,
        /// <summary>
        ///     Challenge expired.
        /// </summary>
        ChallengeExpired,
        /// <summary>
        ///     Challenge has been verified before.
        /// </summary>
        PreviouslyVerified,
        /// <summary>
        ///     Challenge is not a valid base64 string.
        /// </summary>
        ChallengeIsInvalidBase64,
        /// <summary>
        ///     Challenge could be base64-decoded, but could not be parsed as JSON.
        /// </summary>
        ChallengeIsInvalidJson,
        /// <summary>
        ///     Signature is not a valid hex string.
        /// </summary>
        SignatureIsInvalidHexString,
        /// <summary>
        ///     Calculated salt and secret number combination does not match the challenge.
        /// </summary>
        ChallengeDoesNotMatch,
        /// <summary>
        ///     Salt does not have the expected format.
        /// </summary>
        InvalidSalt,
        /// <summary>
        ///     Payload does not match signature.
        /// </summary>
        PayloadDoesNotMatchSignature,
        /// <summary>
        ///     Algorithm does not match the algorithm that was configured.
        /// </summary>
        AlgorithmDoesNotMatch
    }

    /// <summary>
    ///     Possible error codes for invalid spam-filtered challenges.
    /// </summary>
    public enum AltchaSpamFilteredValidationErrorCode
    {
        /// <summary>
        ///     Default error code for when validation succeeded.
        /// </summary>
        NoError,
        /// <summary>
        ///     Challenge expired.
        /// </summary>
        ChallengeExpired,
        /// <summary>
        ///     Challenge has been verified before.
        /// </summary>
        PreviouslyVerified,
        /// <summary>
        ///     Challenge is not a valid base64 string.
        /// </summary>
        ChallengeIsInvalidBase64,
        /// <summary>
        ///     Challenge could be base64-decoded, but could not be parsed as JSON.
        /// </summary>
        ChallengeIsInvalidJson,
        /// <summary>
        ///     Signature is not a valid hex string.
        /// </summary>
        SignatureIsInvalidHexString,
        /// <summary>
        ///     Calculated salt and secret number combination does not match the challenge.
        /// </summary>
        ChallengeDoesNotMatch,
        /// <summary>
        ///     Salt does not have the expected format.
        /// </summary>
        InvalidSalt,
        /// <summary>
        ///     Form submission has expired.
        /// </summary>
        FormSubmissionExpired,
        /// <summary>
        ///     Form submission was not successfully verified by ALTCHA's API.
        /// </summary>
        FormSubmissionNotVerified,
        /// <summary>
        ///     This form's field values do not match what was verified by ALTCHA API's spam filter.
        /// </summary>
        FormFieldValuesDontMatch,
        /// <summary>
        ///     This form's fields do not match what was verified by ALTCHA API's spam filter.
        /// </summary>
        FormFieldsDontMatch,
        /// <summary>
        ///     Payload does not match signature.
        /// </summary>
        PayloadDoesNotMatchSignature,
        /// <summary>
        ///     Algorithm does not match the algorithm that was configured.
        /// </summary>
        AlgorithmDoesNotMatch
    }

    /// <summary>
    ///     Possible error codes for unsolved challenges.
    /// </summary>
    public enum AltchaSolverErrorCode
    {
        /// <summary>
        ///     Default error code for when validation succeeded.
        /// </summary>
        NoError,
        /// <summary>
        ///     Challenge is not a valid hex string.
        /// </summary>
        ChallengeIsInvalidHexString,
        /// <summary>
        ///     Signature is empty.
        /// </summary>
        SignatureIsEmpty,
        /// <summary>
        ///     Salt is empty.
        /// </summary>
        SaltIsEmpty,
        /// <summary>
        ///     Maximum number should be greater than zero.
        /// </summary>
        InvalidMaxNumber,
        /// <summary>
        ///     Could not solve the challenge. Is the maximum number greater than the secret number?
        /// </summary>
        CouldNotSolveChallenge,
        /// <summary>
        ///     Algorithm is not supported.
        /// </summary>
        AlgorithmNotSupported,
        /// <summary>
        ///     Challenge expired.
        /// </summary>
        ChallengeExpired
    }
}
