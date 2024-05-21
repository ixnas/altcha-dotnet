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
            var challenge = _altchaService.Generate();
            challenge.Challenge = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GivenSignatureIsEmpty_WhenSolveCalled_ThenReturnsNegativeResult(string replacement)
        {
            var challenge = _altchaService.Generate();
            challenge.Signature = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GivenSaltIsEmpty_WhenSolveCalled_ThenReturnsNegativeResult(string replacement)
        {
            var challenge = _altchaService.Generate();
            challenge.Salt = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GivenMaxNumberIsInvalid_WhenSolveCalled_ThenReturnsNegativeResult(int replacement)
        {
            var challenge = _altchaService.Generate();
            challenge.Maxnumber = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
        }

        [Fact]
        public void GivenMaxNumberIsPositive_WhenSolveCalled_ThenReturnsPositiveResult()
        {
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
        }

        [Fact]
        public void GivenMaxNumberIsLowerThanSecretNumber_WhenSolveCalled_ThenReturnsNegativeResult()
        {
            var challenge = _altchaService.Generate();
            challenge.Maxnumber = 2;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(CommonServiceType.Api)]
        [InlineData(CommonServiceType.Default)]
        public async Task GivenValidChallengeIsSolved_WhenValidated_ThenReturnsPositiveResult(
            CommonServiceType serviceType)
        {
            var service = TestUtils.ServiceFactories[serviceType]
                                   .GetDefaultService();
            var challenge = service.Generate();
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);
            var validationResult = await service.Validate(result.Altcha);

            Assert.True(validationResult.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("SHA-512")]
        [InlineData("owejrij")]
        public void GivenSignatureIsNotSha256_WhenSolveCalled_ThenReturnsNegativeResult(string replacement)
        {
            var challenge = _altchaService.Generate();
            challenge.Algorithm = replacement;
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(CommonServiceType.Api)]
        [InlineData(CommonServiceType.Default)]
        public void GivenChallengeIsValid_WhenSolveCalled_ThenReturnsPositiveResult(
            CommonServiceType serviceType)
        {
            var service = TestUtils.ServiceFactories[serviceType]
                                   .GetDefaultService();
            var challenge = service.Generate();
            var solver = GetDefaultSolver();
            var result = solver.Solve(challenge);

            Assert.True(result.Success);
        }

        [Theory]
        [InlineData(CommonServiceType.Api)]
        [InlineData(CommonServiceType.Default)]
        public void GivenChallengeHasExpired_WhenSolveCalled_ThenReturnsNegativeResult(
            CommonServiceType serviceType)
        {
            var service = TestUtils.ServiceFactories[serviceType]
                                   .GetServiceWithExpiry(10);
            var challenge = service.Generate();
            var solver = GetDefaultSolver();
            _clock.SetOffsetInSeconds(30);
            var result = solver.Solve(challenge);

            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(CommonServiceType.Api)]
        [InlineData(CommonServiceType.Default)]
        public void GivenIgnoreExpiryIsSet_WhenSolvingExpiredChallengeCalled_ThenReturnsPositiveResult(
            CommonServiceType serviceType)
        {
            var service = TestUtils.ServiceFactories[serviceType]
                                   .GetServiceWithExpiry(10);
            var challenge = service.Generate();
            var solver = GetExpiryIgnoringSolver();
            _clock.SetOffsetInSeconds(30);
            var result = solver.Solve(challenge);

            Assert.True(result.Success);
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
