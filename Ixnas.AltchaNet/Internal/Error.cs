using System.Collections.Generic;

namespace Ixnas.AltchaNet.Internal
{
    internal class Error
    {
        public ErrorCode Code { get; private set; }
        private static readonly Dictionary<ErrorCode, string> MessageMappings =
            new()
            {
                [ErrorCode.NoError] = string.Empty,
                [ErrorCode.SignatureIsInvalidHexString] = "Signature is not a valid hex string.",
                [ErrorCode.ChallengeIsInvalidBase64] = "Challenge is not a valid base64 string.",
                [ErrorCode.ChallengeIsInvalidJson] =
                    "Challenge could be base64-decoded, but could not be parsed as JSON.",
                [ErrorCode.InvalidSalt] = "Salt does not have the expected format.",
                [ErrorCode.ChallengeDoesNotMatch] =
                    "Calculated salt and secret number combination does not match the challenge.",
                [ErrorCode.ChallengeExpired] = "Challenge expired.",
                [ErrorCode.PreviouslyVerified] = "Challenge has been verified before.",
                [ErrorCode.PayloadDoesNotMatchSignature] = "Payload does not match signature.",
                [ErrorCode.AlgorithmDoesNotMatch] =
                    "Algorithm does not match the algorithm that was configured.",
                [ErrorCode.ChallengeIsInvalidHexString] = "Challenge is not a valid hex string.",
                [ErrorCode.SignatureIsEmpty] = "Signature is empty.",
                [ErrorCode.SaltIsEmpty] = "Salt is empty.",
                [ErrorCode.InvalidMaxNumber] = "Maximum number should be greater than zero.",
                [ErrorCode.CouldNotSolveChallenge] =
                    "Could not solve the challenge. Is the maximum number greater than the secret number?",
                [ErrorCode.AlgorithmNotSupported] = "Algorithm is not supported."
            };
        private static readonly Dictionary<ErrorCode, AltchaValidationErrorCode> ValidationErrorCodeMappings =
            new()
            {
                [ErrorCode.NoError] = AltchaValidationErrorCode.NoError,
                [ErrorCode.SignatureIsInvalidHexString] =
                    AltchaValidationErrorCode.SignatureIsInvalidHexString,
                [ErrorCode.ChallengeIsInvalidBase64] = AltchaValidationErrorCode.ChallengeIsInvalidBase64,
                [ErrorCode.ChallengeIsInvalidJson] = AltchaValidationErrorCode.ChallengeIsInvalidJson,
                [ErrorCode.InvalidSalt] = AltchaValidationErrorCode.InvalidSalt,
                [ErrorCode.ChallengeDoesNotMatch] = AltchaValidationErrorCode.ChallengeDoesNotMatch,
                [ErrorCode.ChallengeExpired] = AltchaValidationErrorCode.ChallengeExpired,
                [ErrorCode.PreviouslyVerified] = AltchaValidationErrorCode.PreviouslyVerified,
                [ErrorCode.PayloadDoesNotMatchSignature] =
                    AltchaValidationErrorCode.PayloadDoesNotMatchSignature,
                [ErrorCode.AlgorithmDoesNotMatch] = AltchaValidationErrorCode.AlgorithmDoesNotMatch
            };
        private static readonly Dictionary<ErrorCode, AltchaSolverErrorCode> SolverErrorCodeMappings =
            new()
            {
                [ErrorCode.NoError] = AltchaSolverErrorCode.NoError,
                [ErrorCode.ChallengeIsInvalidHexString] = AltchaSolverErrorCode.ChallengeIsInvalidHexString,
                [ErrorCode.SignatureIsEmpty] = AltchaSolverErrorCode.SignatureIsEmpty,
                [ErrorCode.SaltIsEmpty] = AltchaSolverErrorCode.SaltIsEmpty,
                [ErrorCode.InvalidMaxNumber] = AltchaSolverErrorCode.InvalidMaxNumber,
                [ErrorCode.CouldNotSolveChallenge] = AltchaSolverErrorCode.CouldNotSolveChallenge,
                [ErrorCode.AlgorithmNotSupported] = AltchaSolverErrorCode.AlgorithmNotSupported,
                [ErrorCode.ChallengeExpired] = AltchaSolverErrorCode.ChallengeExpired
            };
        private string _message;

        public static Error Create(ErrorCode code)
        {
            return new Error
            {
                Code = code,
                _message = MessageMappings[code]
            };
        }

        public AltchaValidationResult ToValidationResult()
        {
            return new AltchaValidationResult
            {
                IsValid = Code == ErrorCode.NoError,
                ValidationError = new AltchaValidationError
                {
                    Code = ValidationErrorCodeMappings[Code],
                    Message = _message
                }
            };
        }

        public AltchaSolverResult ToSolverResult(string altcha = null)
        {
            return new AltchaSolverResult
            {
                Success = Code == ErrorCode.NoError,
                Altcha = altcha,
                Error = new AltchaSolverError
                {
                    Code = SolverErrorCodeMappings[Code],
                    Message = _message
                }
            };
        }
    }

    internal enum ErrorCode
    {
        NoError,
        ChallengeExpired,
        PreviouslyVerified,
        ChallengeIsInvalidBase64,
        ChallengeIsInvalidJson,
        SignatureIsInvalidHexString,
        ChallengeDoesNotMatch,
        InvalidSalt,
        PayloadDoesNotMatchSignature,
        AlgorithmDoesNotMatch,
        ChallengeIsInvalidHexString,
        SignatureIsEmpty,
        SaltIsEmpty,
        InvalidMaxNumber,
        CouldNotSolveChallenge,
        AlgorithmNotSupported
    }
}
