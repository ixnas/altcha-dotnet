using System.Security.Cryptography;
using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(Altcha.CreateServiceBuilder()
                                    .UseSha256(GenerateKey())
                                    .UseInMemoryStore()
                                    .SetExpiryInSeconds(5)
                                    .Build());

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/challenge",
           (AltchaService service) => service.Generate());

app.MapPost("/verify",
            async (AltchaService service, [FromForm] string altcha) => new
            {
                (await service.Validate(altcha)).IsValid
            })
   .DisableAntiforgery();

app.Run();

byte[] GenerateKey()
{
    var key = new byte[64];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(key);
        return key;
    }
}
