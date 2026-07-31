# Altcha.NET

[![Build status](https://ci.sjoerdscheffer.nl/job/Altcha.NET/job/main/badge/icon?style=flat-square)](https://ci.sjoerdscheffer.nl/job/Altcha.NET/job/main/)
[![Nuget version](https://ci.sjoerdscheffer.nl/job/Altcha.NET/job/main/badge/icon?config=nugetBadge&style=flat-square)](https://www.nuget.org/packages/Ixnas.AltchaNet)

C# implementation of the [ALTCHA](http://altcha.org) challenge.

**Features**

- Compatible with the [latest ALTCHA client-side widget](https://altcha.org/docs/v2/widget-v3/)
- Independent of ASP.NET (Core)
- Generates and validates self-hosted challenges
- Solves remotely hosted challenges, enabling [machine-to-machine ALTCHA](https://altcha.org/docs/v2/m2m-altcha/)
- Replay attack prevention by denying previously verified challenges
- Expiring challenges

## Contents

- [Installation](#installation)
- [Using self-hosted challenges](#using-self-hosted-challenges)
    - [Set up](#set-up)
        - [Key](#key)
        - [Store](#store)
    - [Usage](#usage)
        - [Generating a challenge](#generating-a-challenge)
        - [Validating a response](#validating-a-response)
- [Solving challenges](#solving-challenges)
    - [Set up](#set-up-2)
    - [Usage](#usage-2)
- [Example](#example)
- [Contributing](#contributing)
- [License](#license)

## Installation

This library is available on NuGet, so you can add it to your project as follows:

```
dotnet add package Ixnas.AltchaNet
```

## Using self-hosted challenges

### Set up

First make sure you've [set up the front-end widget](https://altcha.org/docs/v2/widget-v3/#developer-experience)
to use your challenge endpoint.

The entrypoint of this library contains a method for building and configuring a service.
This service can be used to create ALTCHA challenges and validate their responses.
The most basic configuration looks like this:

```csharp
var altchaService = Altcha.CreateService(new AltchaSha256Configuration
{
    Key = AltchaKey.FromBytes(key),
    StoreFactory = storeFactory,
});
```

Here is a description of the different configuration options.

| Property       | Description                                                                                                                                                                                                                                                         |
|----------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `StoreFactory` | (Required) Configures a store factory to use for previously verified ALTCHA responses. Used to prevent replay attacks.                                                                                                                                              |
| `Key`          | (Required) Configures the SHA-256 algorithm for hashing and signing. Must be 64 bytes long. Currently the only supported algorithm.                                                                                                                                 |
| `Complexity`   | (Optional) Overrides the default complexity to tweak the amount of computational effort a client has to put in. See [ALTCHA's documentation](https://playground.altcha.org/#/about) for more information (default counter range 50,000 - 100,000 with a cost of 1). |
| `Expiry`       | (Optional) Overrides the default time it takes for a challenge to expire (default 120 seconds).                                                                                                                                                                     |

#### Key

The library requires a key to sign and verify ALTCHA challenges.
You can use a random number generator from .NET to create one for you:

```csharp
var key = new byte[64];
using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(key);
}
```

#### Store

The library requires a store implementation to store previously verified challenge responses.
You can use anything persistent, like a database or a file.
As long as it implements the `IAltchaChallengeStore` interface, it will work.

You can use `expiryUtc` to periodically remove expired challenges from your store.

As an example, the bundled in-memory store looks similar to this:

```csharp
public class InMemoryStore : IAltchaChallengeStore
{
    private class StoredChallenge
    {
        public string Challenge { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
    }

    private readonly List<StoredChallenge> _stored = new List<StoredChallenge>();

    public Task Store(string challenge, DateTimeOffset expiryUtc, CancellationToken cancellationToken)
    {
        var challengeToStore = new StoredChallenge
        {
            Challenge = challenge,
            ExpiryUtc = expiryUtc
        };
        _stored.Add(challengeToStore);
        return Task.CompletedTask;
    }

    public Task<bool> Exists(string challenge, CancellationToken cancellationToken)
    {
        _stored.RemoveAll(storedChallenge => storedChallenge.ExpiryUtc <= DateTimeOffset.UtcNow);
        var exists = _stored.Exists(storedChallenge => storedChallenge.Challenge == challenge);
        return Task.FromResult(exists);
    }
}
```

If you're using a short-lived object to access your database (like a request-scoped Entity Framework DbContext), it is
recommended to provide a factory function for the store instead of an instance.

### Usage

#### Generating a challenge

To generate a challenge:

```csharp
var challenge = altchaService.Generate();
```

The `challenge` object can be serialized to JSON for the client to use.
Read [ALTCHA's documentation](https://altcha.org/docs/v2/widget-integration/#using-altcha-widget) on how to use such a
JSON object.

It's possible to override configuration options by passing an anonymous function to change the configuration.
This can be useful when implementing
a [dynamic complexity](https://altcha.org/docs/v2/complexity/#recommended-practices)
strategy, for example.

```csharp
var challenge = altchaService.Generate(configuration => configuration with
{
    Complexity = configuration.Complexity with 
    {
        Counter = new AltchaComplexityCounterRange(200000, 300000),
    },
    Expiry = AltchaExpiry.FromSeconds(300),
});
```

The updated configuration will be used for this single call only.

#### Validating a response

To validate a response:

```csharp
var validationResult = await altchaService.Validate(altcha, cancellationToken);
if (!validationResult.IsValid)
{
    _logger.LogInformation(validationResult.ValidationError.Message);
    /* ... */
}
```

The `altcha` parameter can either be a base64-encoded JSON string (like the raw value of the `altcha` field in a
submitted form), or an already decoded and deserialized `AltchaResponse` object.

The `cancellationToken` parameter can be passed if the service was set up with a `IAltchaCancellableChallengeStore`.
The cancellation token can cancel queries and updates to the store implementation.

## Solving challenges

### Set up

The entrypoint of this library contains a method for creating solver instances. The most basic configuration looks like
this:

```csharp
var altchaSolver = Altcha.CreateSolver();

// Or

var altchaSolver = Altcha.CreateSolver(new AltchaSolverConfiguration()
{
    IgnoreExpiry = true,
});
```

Here is a description of the different configuration options.

| Property         | Description                                                                     |
|------------------|---------------------------------------------------------------------------------|
| `IgnoreExpiry` | (Optional) Disables checking for expiry before attempting to solve a challenge. |

### Usage

To solve a challenge, first make sure you have a deserialized `AltchaChallange` object to solve. Then you can solve the
challenge as follows:

```csharp
var solverResult = altchaSolver.Solve(challenge);

if (!solverResult.Success)
{
    _logger.LogInformation(solverResult.Error.Message);
    /* ... */
}

var formWithAltcha = new
{
    SomeFormField = "some text",
    Altcha = solverResult.Altcha
};
```

This example attaches the solution from `solverResult.Altcha` to a form object as the "altcha" field.

## Example

The `AspNetCoreExample` project contains a few examples for generating, solving and validating challenges.

## Contributing

Bug reports, fixes, ideas and suggestions are always welcome! Feel free to reach out by creating new issues, and I'll
try to respond as soon as I can.

## License

See [LICENSE.txt](https://github.com/ixnas/altcha-dotnet/blob/main/LICENSE.txt)
