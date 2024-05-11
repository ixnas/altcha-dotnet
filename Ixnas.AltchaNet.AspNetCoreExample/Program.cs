using System.Security.Cryptography;
using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var apiSecret = builder.Configuration.GetValue<string>("ApiSecret");

builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
builder.Services.AddSingleton(Altcha.CreateServiceBuilder()
                                    .UseSha256(GenerateKey())
                                    .UseInMemoryStore()
                                    .SetExpiryInSeconds(5)
                                    .Build());
builder.Services.AddSingleton(Altcha.CreateApiServiceBuilder()
                                    .UseApiSecret(apiSecret)
                                    .UseInMemoryStore()
                                    .Build());

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseMvc();

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
