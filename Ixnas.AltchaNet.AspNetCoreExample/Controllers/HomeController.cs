using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Ixnas.AltchaNet.AspNetCoreExample.Controllers;

[Route("/")]
public class HomeController : Controller
{
    public class SpamFilterFormModel
    {
        public string? Email { get; set; }
        public string? Something { get; set; }
        public string? Text { get; set; }
        public string? Altcha { get; set; }
    }

    public class MachineToMachineViewModel
    {
        public string? ChallengeUrl { get; init; }
        public string? ValidationUrl { get; init; }
        public string? Challenge { get; init; }
        public string? SolverResult { get; init; }
        public string? ValidationResult { get; init; }
    }

    public class ViewModel
    {
        public string ApiKey { get; set; } = string.Empty;
    }

    private readonly AltchaApiService _apiService;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AltchaService _service;
    private readonly AltchaSolver _solver;

    public HomeController(AltchaService service,
                          AltchaApiService apiService,
                          IConfiguration configuration,
                          AltchaSolver solver,
                          IHttpClientFactory httpClientFactory)
    {
        _service = service;
        _apiService = apiService;
        _configuration = configuration;
        _solver = solver;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public ActionResult Index()
    {
        var viewModel = new ViewModel
        {
            ApiKey = _configuration["ApiKey"] ?? string.Empty
        };
        return View(viewModel);
    }

    [HttpGet("/challengeSelfHosted")]
    public AltchaChallenge ChallengeSelfHosted()
    {
        return _service.Generate();
    }

    [HttpPost("/verifySelfHosted")]
    public async Task<AltchaValidationResult> VerifySelfHosted(CancellationToken cancellationToken,
                                                               [FromForm] string altcha)
    {
        return await _service.Validate(altcha, cancellationToken);
    }

    [HttpPost("/verifyApiRegular")]
    public async Task<AltchaValidationResult> VerifyApiRegular(CancellationToken cancellationToken,
                                                               [FromForm] string altcha)
    {
        return await _apiService.Validate(altcha, cancellationToken);
    }

    [HttpPost("/verifyApiSpamFiltered")]
    public async Task<AltchaSpamFilteredValidationResult> VerifyApiSpamFiltered(
        CancellationToken cancellationToken,
        [FromForm] SpamFilterFormModel spamFilterFormModel)
    {
        return await _apiService.ValidateSpamFilteredForm(spamFilterFormModel, cancellationToken);
    }

    [HttpGet("/simulateMachineToMachine")]
    public async Task<ActionResult> SimulateMachineToMachine()
    {
        var challengeUrl = "https://localhost:7013/challengeSelfHosted";
        var validationUrl = "https://localhost:7013/verifySelfHosted";
        var httpClient = _httpClientFactory.CreateClient();

        // 1. Get challenge from remote server.
        var challenge = await httpClient.GetAsync(challengeUrl);
        var challengeObject = await challenge.Content.ReadFromJsonAsync<AltchaChallenge>();

        // 2. Solve challenge.
        var solution = _solver.Solve(challengeObject);

        // 3. Post form with solution to remote server.
        var validationFormContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("altcha", solution.Altcha)
        });
        var validation = await httpClient.PostAsync(validationUrl,
                                                    validationFormContent);
        var validationObject = await validation.Content.ReadFromJsonAsync<AltchaValidationResult>();

        // Preparing view
        var serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var challengeJson = JsonSerializer.Serialize(challengeObject, serializerOptions);
        var validationResultJson = JsonSerializer.Serialize(validationObject, serializerOptions);

        var viewModel = new MachineToMachineViewModel
        {
            ChallengeUrl = challengeUrl,
            ValidationUrl = validationUrl,
            Challenge = challengeJson,
            SolverResult = solution.Altcha,
            ValidationResult = validationResultJson
        };

        return View(viewModel);
    }
}
