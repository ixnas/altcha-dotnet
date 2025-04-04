using Ixnas.AltchaNet;
using Ixnas.AltchaNet.AspNetCoreExample.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Read self-hosted key and API secret from appsettings.json
var selfHostedKeyBase64 = builder.Configuration.GetValue<string>("SelfHostedKey");
var selfHostedKey = Convert.FromBase64String(selfHostedKeyBase64!);
var apiSecret = builder.Configuration.GetValue<string>("ApiSecret");

// Initialize database using EF Core with SQLite in-memory.
var sqliteConnection = new SqliteConnection("datasource=:memory:");
sqliteConnection.Open();

// Add challenge store.
builder.Services.AddDbContext<ExampleDbContext>(options =>
                                                    options.UseSqlite(sqliteConnection));
builder.Services.AddScoped<IAltchaCancellableChallengeStore, AltchaChallengeStore>();

// Add HttpClient for machine-to-machine challenges (ignoring SSL issues for this example).
builder.Services.AddHttpClient(Options.DefaultName, _ => { })
       .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
       {
           ClientCertificateOptions = ClientCertificateOption.Manual,
           ServerCertificateCustomValidationCallback =
               (_, _, _, _) => true
       });

// Add Altcha services.
builder.Services.AddScoped(sp => Altcha.CreateServiceBuilder()
                                       .UseSha256(selfHostedKey)
                                       .UseStore(sp.GetService<IAltchaCancellableChallengeStore>)
                                       .SetExpiry(AltchaExpiry.FromSeconds(5))
                                       .Build());
builder.Services.AddScoped(sp => Altcha.CreateApiServiceBuilder()
                                       .UseApiSecret(apiSecret)
                                       .UseStore(sp.GetService<IAltchaCancellableChallengeStore>)
                                       .Build());
builder.Services.AddScoped(_ => Altcha.CreateSolverBuilder()
                                      .Build());

builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();
app.UseMvc();

// Create database table.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
