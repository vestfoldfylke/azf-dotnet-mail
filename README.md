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
        "Serilog_MinimumLevel_Override_Microsoft_Hosting": "Warning",
        "Serilog_MinimumLevel_Override_Microsoft_AspNetCore": "Warning",
        "Serilog_MinimumLevel_Override_OpenApiTriggerFunction": "Warning",
        "API_BaseUrl": "https://www.example.com/v1/",
        "API_AccessToken": "super secret token"
    }
}
```

## Usage

For full input / output documentation, please refer to the [OpenAPI documentation](https://swagger-link)

### Send email - POST `/api/send`

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
  "from": "string",
  "to": [
    "string"
  ],
  "cc": [
    "string"
  ],
  "bcc": [
    "string"
  ],
  "subject": "string",
  "html": "string",
  "text": "string",
  "attachments": [
    {
      "data": "string",
      "url": "string",
      "name": "string",
      "type": "string"
    }
  ],
  "extra": {
    "additionalProp1": "string",
    "additionalProp2": "string",
    "additionalProp3": "string"
  },
  "template": {
    "templateName": "string",
    "templateData": {
      "body": "string",
      "signature": {
        "name": "string",
        "title": "string",
        "department": "string",
        "company": "string",
        "phone": "string",
        "mobile": "string",
        "webpage": "string"
      }
    }
  }
}
```

### Send bulk email - POST `/api/bulksend`

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
  "from": "string",
  "bulkRecipients": [
    "string"
  ],
  "subject": "string",
  "html": "string",
  "text": "string",
  "attachments": [
    {
      "data": "string",
      "url": "string",
      "name": "string",
      "type": "string"
    }
  ],
  "extra": {
    "additionalProp1": "string",
    "additionalProp2": "string",
    "additionalProp3": "string"
  },
  "template": {
    "templateName": "string",
    "templateData": {
      "body": "string",
      "signature": {
        "name": "string",
        "title": "string",
        "department": "string",
        "company": "string",
        "phone": "string",
        "mobile": "string",
        "webpage": "string"
      }
    }
  }
}
```

### Available templates

- `vestfoldfylke`
- `telemarkfylke`

### Get email status - POST `/api/status/{messageId}`

**Headers**
```json
{
  "X-Functions-Key": "<function-key>"
}
```