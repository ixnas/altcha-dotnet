# Altcha.NET

[![Build Status](https://ci.sjoerdscheffer.nl/job/Altcha.NET/job/main/badge/icon?style=flat-square)](https://ci.sjoerdscheffer.nl/job/Altcha.NET/job/main/)

Server-side implementation of the [ALTCHA](http://altcha.org) challenge in C#.

### Features

- Compatible with the [ALTCHA client-side widget](https://altcha.org/docs/website-integration/#using-altcha-widget)
- Independent from ASP.NET (Core)
- Replay attack prevention by denying previously verified challenges
- Expiring challenges

## Disclaimer

I am not part of the ALTCHA project and this is no official implementation. This is merely a compatible C#
implementation of an ALTCHA server. All credits for their CAPTCHA solution go to them!

## Set up

The entrypoint of this library contains a service builder.
This builder configures the service that is used to create ALTCHA challenges and validate their responses.
The most basic configuration looks like this:

```csharp
var altchaService = Altcha.CreateServiceBuilder()
                          .UseSha256(key)
                          .UseStore(store)
                          .Build());
```

Here is a description of the different configuration options.

| Method                                    | Description                                                                                                                                                                  |
|-------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `UseStore(IAltchaChallengeStore store)`   | (Required) Configures a store to use for previously verified ALTCHA responses. Used to prevent replay attacks.                                                               |
| `UseSha256(byte[] key)`                   | (Required) Configures the SHA-256 algorithm for hashing and signing. Currently the only supported algorithm.                                                                 |
| `SetComplexity(int min, int max)`         | (Optional) Overrides the default complexity to tweak the amount of computational effort a client has to put in. See https://altcha.org/docs/complexity/ for more information |
| `SetExpiryInSeconds(int expiryInSeconds)` | (Optional) Overrides the default time it takes for a challenge to expire.                                                                                                    |
| `UseInMemoryStore()`                      | Configures a simple in-memory store for previously verified ALTCHA responses. Should only be used for testing purposes.                                                      |
| `Build()`                                 | Returns a new configured service instance.                                                                                                                                   |

### Key

The library requires a key to sign and verify ALTCHA challenges.
You can use a random number generator from .NET to create one for you:

```csharp
var key = new byte[64];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(key);
    return key;
}
```

### Store

The library requires a store implementation to store previously verified challenge responses.
You can use anything persistent, like a database or a file.
As long as it implements the `IAltchaChallengeStore` interface, it will work.
As an example, the bundled in-memory store (that you shouldn't use) looks like this:

```csharp
internal class InMemoryStore : IAltchaChallengeStore
{
    private readonly List<string> _stored = new List<string>();

    public Task Store(string challenge)
    {
        _stored.Add(challenge);
        return Task.CompletedTask;
    }

    public Task<bool> Exists(string challenge)
    {
        var exists = _stored.Contains(challenge);
        return Task.FromResult(exists);
    }
}
```

## Usage

### Generate challenge

To generate a challenge:

```csharp
var challenge = altchaService.Generate();
```

The `challenge` object can be serialized to JSON for the client to use.
Read [ALTCHA's documentation](https://altcha.org/docs/website-integration/#using-altcha-widget) on how to use such a
JSON object.

### Validate response

To validate a response:

```csharp
var validationResult = await altchaService.Validate(altchaBase64);
if (!validationResult.IsValid)
    /* ... */
```

The `altchaBase64` parameter represents the raw value from the `altcha` field in a submitted form, so you don't need to
convert anything.

You can run and examine the `AspNetCoreExample` project as a minimal example.

## License

See [LICENSE.txt](./LICENSE.txt)

See [LICENSE-ALTCHA.txt](./LICENSE-ALTCHA.txt) for ALTCHA's original license.
