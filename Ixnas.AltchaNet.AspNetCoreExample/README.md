# Altcha.NET example project

This project contains a simple example project using ASP.NET Core with MVC.
It demonstrates self-hosted challenges as well ALTCHA API integration and spam filtering.

## Set up

### API key and secret

Make sure you set your API key and secret in `appsettings.json`

```json
{
  "ApiKey": "ckey_INSERT_KEY_HERE",
  "ApiSecret": "csec_INSERT_SECRET_HERE"
}
```

### Hosts file

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