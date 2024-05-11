using Microsoft.AspNetCore.Mvc;

namespace Ixnas.AltchaNet.AspNetCoreExample.Controllers;

[Route("/")]
public class HomeController : Controller
{
    public class FormModel
    {
        public string? Email { get; set; }
        public string? Something { get; set; }
        public string? Text { get; set; }
        public string? Altcha { get; set; }
    }

    public class ViewModel
    {
        public string ApiKey { get; set; } = string.Empty;
    }

    private readonly IConfiguration _configuration;
    private readonly AltchaService _service;
    private readonly AltchaApiService _apiService;

    public HomeController(AltchaService service, AltchaApiService apiService, IConfiguration configuration)
    {
        _service = service;
        _apiService = apiService;
        _configuration = configuration;
    }

    [HttpGet]
    public ActionResult Index()
    {
        var viewModel = new ViewModel()
        {
            ApiKey = _configuration["ApiKey"] ?? string.Empty
        };
        return View(viewModel);
    }

    [HttpPost("/verifyApiSelfHosted")]
    public async Task<AltchaValidationResult> VerifySelfHosted([FromForm] string altcha)
    {
        return await _service.Validate(altcha);
    }

    [HttpPost("/verifyApiRegular")]
    public async Task<AltchaValidationResult> VerifyApiRegular([FromForm] string altcha)
    {
        return await _apiService.Validate(altcha);
    }

    [HttpPost("/verifyApiSpamFiltered")]
    public async Task<AltchaSpamFilteredValidationResult> VerifyApiSpamFiltered([FromForm] FormModel formModel)
    {
        return await _apiService.ValidateSpamFilteredForm(formModel);
    }
}
