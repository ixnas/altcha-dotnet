using System;
using System.Threading.Tasks;
using Ixnas.AltchaNet.Tests.Abstractions;
using Ixnas.AltchaNet.Tests.Fakes;
using Xunit;

namespace Ixnas.AltchaNet.Tests
{
    public class SolverTests
    {
        private readonly AltchaService _altchaService = Altcha.CreateServiceBuilder()
                                                              .UseSha256(TestUtils.GetKey())
                                                              .UseInMemoryStore()
                                                              .SetComplexity(3, 4)
                                                              .Build();
        private readonly ClockFake _clock = new ClockFake();

        [Fact]
        public void GivenChallengeIsNull_WhenSolveCalled_ThrowsException()
        {
            var solver = GetDefaultSolver();
            Assert.Throws<ArgumentNullException>(() => solver.Solve(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("random")]
        public void GivenChallengeStringIsInvalid_WhenSolveCalled_ThenReturnsNegativeResult(
            string replacement)
        {
            const string expectedErrorMessage = "Challenge is not a valid hex string.";
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.ChallengeIsInvalidHexString;

            var challenge = _altchaService.Generate();
            challenge.Challenge = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GivenSignatureIsEmpty_WhenSolveCalled_ThenReturnsNegativeResult(string replacement)
        {
            const string expectedErrorMessage = "Signature is empty.";
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.SignatureIsEmpty;

            var challenge = _altchaService.Generate();
            challenge.Signature = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GivenSaltIsEmpty_WhenSolveCalled_ThenReturnsNegativeResult(string replacement)
        {
            const string expectedErrorMessage = "Salt is empty.";
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.SaltIsEmpty;

            var challenge = _altchaService.Generate();
            challenge.Salt = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GivenMaxNumberIsInvalid_WhenSolveCalled_ThenReturnsNegativeResult(int replacement)
        {
            const string expectedErrorMessage = "Maximum number should be greater than zero.";
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.InvalidMaxNumber;

            var challenge = _altchaService.Generate();
            challenge.Maxnumber = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Fact]
        public void GivenMaxNumberIsPositive_WhenSolveCalled_ThenReturnsPositiveResult()
        {
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.NoError;

            var challenge = Altcha.CreateServiceBuilder()
                                  .UseSha256(TestUtils.GetKey())
                                  .UseInMemoryStore()
                                  .SetComplexity(1, 1)
                                  .UseInMemoryStore()
                                  .Build()
                                  .Generate();
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Fact]
        public void GivenMaxNumberIsLowerThanSecretNumber_WhenSolveCalled_ThenReturnsNegativeResult()
        {
            const string expectedErrorMessage =
                "Could not solve the challenge. Is the maximum number greater than the secret number?";
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.CouldNotSolveChallenge;

            var challenge = _altchaService.Generate();
            challenge.Maxnumber = 2;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Theory]
        [InlineData(CommonServiceType.Api)]
        [InlineData(CommonServiceType.Default)]
        public async Task GivenValidChallengeIsSolved_WhenValidated_ThenReturnsPositiveResult(
            CommonServiceType serviceType)
        {
            const AltchaValidationErrorCode expectedErrorCode = AltchaValidationErrorCode.NoError;

            var service = TestUtils.ServiceFactories[serviceType]
                                   .GetDefaultService();
            var challenge = service.Generate();
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);
            var validationResult = await service.Validate(result.Altcha);

            Assert.True(validationResult.IsValid);
            Assert.Equal(string.Empty, validationResult.ValidationError.Message);
            Assert.Equal(expectedErrorCode, validationResult.ValidationError.Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("SHA-512")]
        [InlineData("owejrij")]
        public void GivenAlgorithmIsNotSha256_WhenSolveCalled_ThenReturnsNegativeResult(string replacement)
        {
            const string expectedErrorMessage = "Algorithm is not supported.";
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.AlgorithmNotSupported;

            var challenge = _altchaService.Generate();
            challenge.Algorithm = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Theory]
        [InlineData(CommonServiceType.Api)]
        [InlineData(CommonServiceType.Default)]
        public void GivenChallengeIsValid_WhenSolveCalled_ThenReturnsPositiveResult(
            CommonServiceType serviceType)
        {
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.NoError;

            var service = TestUtils.ServiceFactories[serviceType]
                                   .GetDefaultService();
            var challenge = service.Generate();
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.True(result.Success);

            Assert.Equal(string.Empty, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Theory]
        [InlineData(CommonServiceType.Api)]
        [InlineData(CommonServiceType.Default)]
        public void GivenChallengeHasExpired_WhenSolveCalled_ThenReturnsNegativeResult(
            CommonServiceType serviceType)
        {
            const string expectedErrorMessage = "Challenge expired.";
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.ChallengeExpired;

            var service = TestUtils.ServiceFactories[serviceType]
                                   .GetServiceWithExpiry(10);
            var challenge = service.Generate();
            var solver = GetDefaultSolver();
            _clock.SetOffsetInSeconds(30);
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
            Assert.Equal(expectedErrorMessage, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Theory]
        [InlineData(CommonServiceType.Api)]
        [InlineData(CommonServiceType.Default)]
        public void GivenIgnoreExpiryIsSet_WhenSolvingExpiredChallengeCalled_ThenReturnsPositiveResult(
            CommonServiceType serviceType)
        {
            const AltchaSolverErrorCode expectedErrorCode = AltchaSolverErrorCode.NoError;

            var service = TestUtils.ServiceFactories[serviceType]
                                   .GetServiceWithExpiry(10);
            var challenge = service.Generate();
            var solver = GetExpiryIgnoringSolver();
            _clock.SetOffsetInSeconds(30);
            var result = solver.Solve(challenge);

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.Error.Message);
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        private AltchaSolver GetDefaultSolver()
        {
            return Altcha.CreateSolverBuilder()
                         .UseClock(_clock)
                         .Build();
        }

        private AltchaSolver GetExpiryIgnoringSolver()
        {
            return Altcha.CreateSolverBuilder()
                         .IgnoreExpiry()
                         .UseClock(_clock)
                         .Build();
        }
    }
}
