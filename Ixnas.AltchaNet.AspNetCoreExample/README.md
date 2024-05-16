# Altcha.NET example project

This project contains a simple example project using ASP.NET Core with MVC.
It demonstrates self-hosted challenges as well ALTCHA API integration and spam filtering.

## Set up

In `appsettings.json` there are a few settings you should configure.

```json
{
  "SelfHostedKey": "INSERT_BASE64_KEY_HERE",
  "ApiKey": "ckey_INSERT_KEY_HERE",
  "ApiSecret": "csec_INSERT_SECRET_HERE"
}
```

- If you want to use self-hosted challenges, insert a 64-byte key into `SelfHostedKey`, encoded as base64.
- If you want to use the API integration, insert your API key into `ApiKey`, and your API secret into `ApiSecret`.

### Hosts file (API integration only)

ALTCHA's API depends on the `Referer` header being set to the domain you registered your token with.

If you run this project locally and you'd like to test the integration with ALTCHA's API, you should add an alias to
your hosts file for this domain.

- On macOS/Linux, this file will be at `/etc/hosts`.
- On Windows, this file will be at `C:\Windows\System32\Drivers\etc\hosts`.

Modify the line that looks like this:

```
127.0.0.1       localhost
```

To this:

```
127.0.0.1       localhost yourdomain.com
```

## Usage

Start the application and go to `https://yourdomain.com:7013/`.
If you're not using the API integration, you can just go to `https://localhost:7013/`.