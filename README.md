# Azure function API for sending emails through SMTP

## Setup

Create a `local.settings.json` file
```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "AppName": "azf-dotnet-mail",
        "Version": "1.0.0",
        "BetterStack_SourceToken": "Token",
        "BetterStack_Endpoint": "https://endpoint.com",
        "BetterStack_MinimumLevel": "Information",
        "Smtp_Server": "server.com",
        "Smtp_Port": 587,
        "Smtp_Username": "username",
        "Smtp_Password": "password"
    }
}
```

## Usage

### Send email - POST `/api/mail`

**Headers**
```json
{
  "Content-Type": "application/json",
  "X-Functions-Key": "<function-key>"
}
```

**Body**
```json
{
  "from": "from@address.com",
  "recipients": [
    "to@address.com",
    "to2@address.com"
  ],
  "subject": "Subject",
  "body": "Mail text"
}
```