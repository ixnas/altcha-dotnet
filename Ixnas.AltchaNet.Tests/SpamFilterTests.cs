using Ixnas.AltchaNet.Tests.Simulations;

namespace Ixnas.AltchaNet.Tests;

public class SpamFilterTests
{
    [Serializable]
    private class TestForm : AltchaForm
    {
        public string? Email { get; set; }
        public string? Text { get; set; }
        public string? Something { get; set; }
        public string? Altcha { get; set; }
    }

    [Serializable]
    private class TestForm2 : AltchaForm
    {
        public string? Email { get; set; }
        public string? Text { get; set; }
        public string? SomethingElse { get; set; }
        public string? Altcha { get; set; }
    }

    [Serializable]
    private class TestFormWithoutAltcha
    {
        public string? Email { get; set; }
        public string? Something { get; set; }
        public string? Text { get; set; }
    }

    [Serializable]
    private class TestFormWithDifferentAltchaProperty
    {
        public string? Email { get; set; }
        public string? Something { get; set; }
        public string? Text { get; set; }
        public string? AlternativeAltcha { get; set; }
    }

    private const double DefaultSpamThreshold = 2;
    private readonly AltchaApiSimulation _apiSimulation;

    public SpamFilterTests()
    {
        var key = TestUtils.GetApiSecret();
        _apiSimulation = new AltchaApiSimulation(key);
    }

    [Fact]
    public async Task GivenSpamFilteredFormIsNull_WhenValidateSpamFilteredFormCalled_ThrowsException()
    {
        var service = GetDefaultService();
        await Assert.ThrowsAsync<ArgumentNullException>(() => service
                                                            .ValidateSpamFilteredForm<object>(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("weorijfosdlkjaewropj")]
    [InlineData("eyJzb21ldGhpbmciOiJtaXNzaW5nIiwiaGVyZSI6Im5vdyJ9")]
    public async Task GivenAltchaHasInvalidFormat_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
        string altcha)
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        form.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.False(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    [Fact]
    public async Task GivenFormHasNoAltchaProperty_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
    {
        var service = GetDefaultService();
        var form = new TestFormWithoutAltcha
        {
            Email = "name@provider.com",
            Something = "Some text",
            Text = "More text"
        };
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.False(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    [Theory]
    [InlineData("Some text", 2)]
    [InlineData("  Some text  ", 2)]
    [InlineData("Some text", 1.5)]
    [InlineData("  Some text  ", 1.5)]
    public async Task GivenFormIsValid_WhenValidateSpamFilteredFormCalled_ReturnsPositiveResult(
        string validText,
        double score)
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        form.Something = validText;
        var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                         score,
                                                         30,
                                                         true,
                                                         f => f.Altcha);
        form.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.True(result.IsValid);
        Assert.True(result.PassedSpamFilter);
    }

    [Fact]
    public async Task
        GivenFormUsesDifferentAltchaProperty_WhenValidateSpamFilteredFormCalledWithExpression_ReturnsPositiveResult()
    {
        var service = GetDefaultService();
        var form = new TestFormWithDifferentAltchaProperty
        {
            Email = "name@provider.com",
            Something = "Some text",
            Text = "More text"
        };
        var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                         DefaultSpamThreshold,
                                                         30,
                                                         true,
                                                         f => f.AlternativeAltcha);
        form.AlternativeAltcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form, x => x.AlternativeAltcha);

        Assert.True(result.IsValid);
        Assert.True(result.PassedSpamFilter);
    }

    [Fact]
    public async Task GivenFormIsValid_WhenValidateSpamFilteredFormCalledTwice_ReturnsNegativeResult()
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        var altcha = GenerateDefaultSpamFiltered(form);
        form.Altcha = altcha;
        await service.ValidateSpamFilteredForm(form);
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.False(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    [Fact]
    public async Task GivenFormIsExpired_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                         DefaultSpamThreshold,
                                                         -30,
                                                         true,
                                                         f => f.Altcha);
        form.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.False(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    [Fact]
    public async Task GivenFormIsNotVerified_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                         DefaultSpamThreshold,
                                                         30,
                                                         false,
                                                         f => f.Altcha);
        form.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.False(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    [Theory]
    [InlineData("")]
    [InlineData("x")]
    public async Task GivenFormHasInvalidSignature_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
        string prefix)
    {
        await TestMalformedAltcha(signature => prefix + signature[1..]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GivenFormHasNoSignature_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
        string signature)
    {
        await TestMalformedAltcha(_ => signature);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("SHA-512")]
    public async Task GivenFormHasInvalidAlgorithm_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
        string signature)
    {
        await TestMalformedAltcha(null, () => signature);
    }

    [Fact]
    public async Task
        GivenFormHasMalformedVerificationData_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
    {
        await TestMalformedAltcha(null,
                                  null,
                                  verificationData => verificationData.Replace("score=2", "score=x"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task
        GivenFormHasEmptyVerificationData_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
            string verificationData)
    {
        await TestMalformedAltcha(null, null, _ => verificationData);
    }

    [Fact]
    public async Task GivenFormHasSpam_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                         DefaultSpamThreshold + 0.1,
                                                         30,
                                                         true,
                                                         f => f.Altcha);
        form.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.True(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    [Fact]
    public async Task
        GivenFormHasSpamAccordingToCustomThreshold_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult()
    {
        var service = GetServiceWithThreshold(0.5);
        var form = GetDefaultForm();
        var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                         0.6,
                                                         30,
                                                         true,
                                                         f => f.Altcha);
        form.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.True(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task
        GivenFormHasEmptyButMatchingField_WhenValidateSpamFilteredFormCalled_ReturnsPositiveResult(
            string emptyField)
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        form.Something = emptyField;
        var altcha = GenerateDefaultSpamFiltered(form);
        form.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.True(result.IsValid);
        Assert.True(result.PassedSpamFilter);
    }

    [Theory]
    [InlineData("Before", "After")]
    [InlineData("Before", null)]
    [InlineData(null, "After")]
    public async Task
        GivenFormIsHasAlteredFieldValue_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
            string before,
            string after)
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        form.Something = before;
        var altcha = GenerateDefaultSpamFiltered(form);

        form.Altcha = altcha;
        form.Something = after;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.False(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    [Theory]
    [InlineData("Some text", "Some text")]
    [InlineData("Some text", "Something else")]
    [InlineData("Some text", null)]
    [InlineData(null, "Some text")]
    public async Task GivenFormIsHasAlteredField_WhenValidateSpamFilteredFormCalled_ReturnsNegativeResult(
        string before,
        string after)
    {
        var service = GetDefaultService();
        var form1 = GetDefaultForm();
        form1.Something = before;
        var form2 = new TestForm2
        {
            Email = "name@provider.com",
            SomethingElse = after,
            Text = "More text"
        };
        var altcha = GenerateDefaultSpamFiltered(form1);
        form2.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form2);

        Assert.False(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    private async Task TestMalformedAltcha(Func<string, string>? malformedSignatureFn = null,
                                           Func<string>? replaceAlgorithmFn = null,
                                           Func<string, string>? malformVerificationDataFn = null)
    {
        var service = GetDefaultService();
        var form = GetDefaultForm();
        var altcha = _apiSimulation.GenerateSpamFiltered(form,
                                                         DefaultSpamThreshold,
                                                         30,
                                                         true,
                                                         f => f.Altcha,
                                                         malformedSignatureFn,
                                                         replaceAlgorithmFn,
                                                         malformVerificationDataFn);
        form.Altcha = altcha;
        var result = await service.ValidateSpamFilteredForm(form);

        Assert.False(result.IsValid);
        Assert.False(result.PassedSpamFilter);
    }

    private static TestForm GetDefaultForm()
    {
        return new TestForm
        {
            Email = "name@provider.com",
            Something = "Some text",
            Text = "More text"
        };
    }

    private string GenerateDefaultSpamFiltered<T>(T form) where T : AltchaForm
    {
        return _apiSimulation.GenerateSpamFiltered(form,
                                                   DefaultSpamThreshold,
                                                   30,
                                                   true,
                                                   f => f.Altcha);
    }

    private static AltchaApiService GetDefaultService()
    {
        return Altcha.CreateApiServiceBuilder()
                     .UseInMemoryStore()
                     .UseApiSecret(TestUtils.GetApiSecret())
                     .Build();
    }

    private static AltchaApiService GetServiceWithThreshold(double threshold)
    {
        return Altcha.CreateApiServiceBuilder()
                     .UseInMemoryStore()
                     .UseApiSecret(TestUtils.GetApiSecret())
                     .SetMaxSpamFilterScore(threshold)
                     .Build();
    }
}
