# Altcha.NET example project

This project contains a simple example project using ASP.NET Core with MVC.
It demonstrates self-hosted challenges as well ALTCHA API integration and spam filtering.

## Set up

Configure a key in `appsettings.json`. This should be a randomly generated base64 string that decodes to 64 bytes.

```json
{
  "SelfHostedKey": "INSERT_BASE64_KEY_HERE"
}
```

## Usage

Start the application and go to `https://yourdomain.com:7013/`.
If you're not using the API integration, you can just go to `https://localhost:7013/`.