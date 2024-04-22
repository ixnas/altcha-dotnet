using System.Security.Cryptography;
using Altcha.Net;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(Altcha.Net.Altcha.CreateServiceBuilder()
                                    .AddKey(GenerateKey())
                                    .AddInMemoryStore()
                                    .Build());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/challenge",
           (IAltchaService service) => service.GenerateChallenge());

app.MapPost("/verify",
            async (IAltchaService service, [FromForm] string altcha) => new
            {
                (await service.ValidateResponse(altcha)).IsValid
            })
   .DisableAntiforgery();

app.Run();

byte[] GenerateKey()
{
    var key = new byte[64];
    using var rng = new RNGCryptoServiceProvider();
    rng.GetBytes(key);

    return key;
}
