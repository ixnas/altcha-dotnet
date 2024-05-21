# Altcha.NET

[![Build status](https://ci.sjoerdscheffer.nl/job/Altcha.NET/job/main/badge/icon?style=flat-square)](https://ci.sjoerdscheffer.nl/job/Altcha.NET/job/main/)
[![Nuget version](https://ci.sjoerdscheffer.nl/job/Altcha.NET/job/main/badge/icon?config=nugetBadge&style=flat-square)](https://www.nuget.org/packages/Ixnas.AltchaNet)

C# implementation of the [ALTCHA](http://altcha.org) challenge.

**Features**

- Compatible with the [ALTCHA client-side widget](https://altcha.org/docs/website-integration/#using-altcha-widget)
- Independent from ASP.NET (Core)
- Generating and validating self-hosted challenges
- Validating challenges from [ALTCHA's public API](https://altcha.org/docs/api/)
- Validating forms that were spam-filtered by [ALTCHA's spam filter API](https://altcha.org/docs/api/spam-filter-api/)
- Includes client for solving challenges, enabling [machine-to-machine ALTCHA](https://altcha.org/docs/m2m-altcha/)
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
- [Verifying challenges from ALTCHA's API](#verifying-challenges-from-altchas-api)
    - [Set up](#set-up-1)
    - [Usage](#usage-1)
        - [Validating a regular response](#validating-a-regular-response)
        - [Validating a spam filtered form](#validating-a-spam-filtered-form)
- [Solving challenges](#solving-challenges)
    - [Set up](#set-up-2)
    - [Usage](#usage-2)
- [Example](#example)
- [License](#license)

## Installation

This library is available on NuGet, so you can add it to your project as follows:

```
dotnet add package Ixnas.AltchaNet
```

## Using self-hosted challenges

### Set up

First make sure you've [set up the front-end widget](https://altcha.org/docs/website-integration/#using-altcha-widget)
to use your challenge endpoint.

The entrypoint of this library contains a service builder for self-hosted configurations.
This builder configures the service that is used to create ALTCHA challenges and validate their responses.
The most basic configuration looks like this:

```csharp
var altchaService = Altcha.CreateServiceBuilder()
                          .UseSha256(key)
                          .UseStore(storeFactory)
                          .Build();
```

Here is a description of the different configuration options.

| Method                                               | Description                                                                                                                                                                                                                     |
|------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `UseStore(Func<IAltchaChallengeStore> storeFactory)` | (Required) Configures a store factory to use for previously verified ALTCHA responses. Used to prevent replay attacks.                                                                                                          |
| `UseStore(IAltchaChallengeStore store)`              | (Required) Configures a store instance to use for previously verified ALTCHA responses. Used to prevent replay attacks.                                                                                                         |
| `UseSha256(byte[] key)`                              | (Required) Configures the SHA-256 algorithm for hashing and signing. Must be 64 bytes long. Currently the only supported algorithm.                                                                                             |
| `SetComplexity(int min, int max)`                    | (Optional) Overrides the default complexity to tweak the amount of computational effort a client has to put in. See [ALTCHA's documentation](https://altcha.org/docs/complexity/) for more information (default 50000, 100000). |
| `SetExpiryInSeconds(int expiryInSeconds)`            | (Optional) Overrides the default time it takes for a challenge to expire (default 120 seconds).                                                                                                                                 |
| `UseInMemoryStore()`                                 | Configures a simple in-memory store for previously verified ALTCHA responses. Should only be used for testing purposes.                                                                                                         |
| `Build()`                                            | Returns a new configured service instance.                                                                                                                                                                                      |

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
For example, the bundled in-memory store looks similar to this:

```csharp
public class InMemoryStore : IAltchaChallengeStore
{
    private class StoredChallenge
    {
        public string Challenge { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
    }

    private readonly List<StoredChallenge> _stored = new List<StoredChallenge>();

    public Task Store(string challenge, DateTimeOffset expiryUtc)
    {
        var challengeToStore = new StoredChallenge
        {
            Challenge = challenge,
            ExpiryUtc = expiryUtc
        };
        _stored.Add(challengeToStore);
        return Task.CompletedTask;
    }

    public Task<bool> Exists(string challenge)
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
Read [ALTCHA's documentation](https://altcha.org/docs/website-integration/#using-altcha-widget) on how to use such a
JSON object.

#### Validating a response

To validate a response:

```csharp
var validationResult = await altchaService.Validate(altchaBase64);
if (!validationResult.IsValid)
    /* ... */
```

The `altchaBase64` parameter represents the raw value from the `altcha` field in a submitted form, so you don't need to
convert anything.

## Verifying challenges from ALTCHA's API

### Set up

First make sure you've [set up the front-end widget](https://altcha.org/docs/api/challenge-api/#using-with-the-widget)
to use the API.

The entrypoint of this library contains a different service builder for integrating with ALTCHA's API.
The most basic configuration looks like this:

```csharp
var altchaApiService = Altcha.CreateApiServiceBuilder()
                             .UseApiSecret(secret)
                             .UseStore(storeFactory)
                             .Build();
```

Here is a description of the different configuration options.

| Method                                               | Description                                                                                                               |
|------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------|
| `UseStore(Func<IAltchaChallengeStore> storeFactory)` | (Required) Configures a store factory to use for previously verified ALTCHA responses. Used to prevent replay attacks.    |
| `UseStore(IAltchaChallengeStore store)`              | (Required) Configures a store to use for previously verified ALTCHA responses. Used to prevent replay attacks.            |
| `UseApiSecret(string secret)`                        | (Required) Configures the API secret used to validate challenges from ALTCHA's API. Starts with either "sec_" or "_csec". |
| `SetMaxSpamFilterScore(double score)`                | (Optional) Overrides the default maximum score that a spam filtered form may have before it's rejected (default 2).       |
| `UseInMemoryStore()`                                 | Configures a simple in-memory store for previously verified ALTCHA responses. Should only be used for testing purposes.   |
| `Build()`                                            | Returns a new configured service instance.                                                                                |

The store uses the [same interface](#store) as it does for the self-hosted service.
You can even use the same instance if you like.

### Usage

#### Validating a regular response

To validate a regular response:

```csharp
var validationResult = await altchaApiService.Validate(altchaBase64);
if (!validationResult.IsValid)
    /* ... */
```

This works the same way as [self-hosted validation](#validating-a-response).
Challenges generated by the self-hosted service can not be validated by the API service, or vice versa.

#### Validating a spam filtered form

To validate a spam filtered form, you need an object that represents the form fields as public string properties.
By default, the library looks for a public string property named `Altcha` that should contain the raw value from
the `altcha` field in a submitted form.
A form class could look like this:

```csharp
public class ExampleForm
{
    public string Altcha { get; set; }
    public string Email { get; set; }
    public string Text { get; set; }
}
```

To validate the form:

```csharp
var validationResult = await altchaApiService.ValidateSpamFilteredForm(form);
if (!validationResult.IsValid)
    /* ... */

if (!validationResult.PassedSpamFilter)
    /* ... */
```

If you prefer to use a different property for the ALTCHA payload, you can use a member expression to select it:

```csharp
var validationResult = await altchaApiService.ValidateSpamFilteredForm(form, x => x.AnotherProperty);
```

The result's `IsValid` property tells you whether the form data, verification data and the signature are valid.
You should probably reject the form submission if this is not the case.

The result's `PassedSpamFilter` property tells you whether the form data successfully passed through the spam filter.
You might want to keep the form submission and mark it as spam in your application for manual approval.

## Solving challenges

### Set up

The entrypoint of this library contains a builder for creating solver instances. The most basic configuration looks like
this:

```csharp
var altchaSolver = Altcha.CreateSolverBuilder()
                         .Build();
```

Here is a description of the different configuration options.

| Method           | Description                                                                     |
|------------------|---------------------------------------------------------------------------------|
| `IgnoreExpiry()` | (Optional) Disables checking for expiry before attempting to solve a challenge. |
| `Build()`        | Returns a new configured solver instance.                                       |

### Usage

To solve a challenge, first make sure you have a deserialized `AltchaChallange` object to solve. Then you can solve the
challenge as follows:

```csharp
var solverResult = altchaSolver.Solve(challenge);

if (!solverResult.Success)
    /* ... */

var formWithAltcha = new
{
    SomeFormField = "some text",
    Altcha = solverResult.Altcha
};
```

This example attaches the solution from `solverResult.Altcha` to a form object as the "altcha" field.

## Example

The `AspNetCoreExample` project contains a few examples for generating, solving and validating challenges.

## License

See [LICENSE.txt](https://github.com/ixnas/altcha-dotnet/blob/main/LICENSE.txt)

See [LICENSE-ALTCHA.txt](https://github.com/ixnas/altcha-dotnet/blob/main/LICENSE-ALTCHA.txt) for ALTCHA's original
license.
