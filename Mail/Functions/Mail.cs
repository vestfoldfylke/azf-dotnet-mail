using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Mail.Contracts;
using Mail.Services;
using Mail.Validation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace Mail.Functions;

public class Mail
{
    private readonly ILogger<Mail> _logger;
    private readonly IMailSender _mailSender;

    public Mail(ILogger<Mail> logger, IMailSender mailSender)
    {
        _logger = logger;
        _mailSender = mailSender;
    }

    [Function("SendMail")]
    [OpenApiOperation(operationId: "SendMail")]
    [OpenApiSecurity("Authentication", SecuritySchemeType.ApiKey, Name = "X-Functions-Key", In = OpenApiSecurityLocationType.Header)]
    [OpenApiRequestBody("application/json", typeof(Message),
        Description = "JSON request body containing message details.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string),
        Description = "The OK response message containing a JSON result")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string),
        Description = "Bad request")]
    public async Task<IActionResult> SendMail([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
        [FromBody] Message message)
    {
        _logger.LogInformation("Sending mail message: {@message}", message);
        
        if (message.To is null || !message.To.Any())
        {
            message.To = [..message.Recipients];
        }
        
        MessageValidator validator = new();
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid)
        {
            return new BadRequestObjectResult(validationResult.Errors.Select(failure => failure.ErrorMessage));
        }
        
        (HttpStatusCode statusCode, string result) = await _mailSender.SendMail(message);
        if (string.IsNullOrEmpty(result))
        {
            return new BadRequestObjectResult("No content returned from mail service");
        }
        
        (JsonArray? arrayContent, JsonNode? objectContent, string? stringContent) = GetReturnContent(result);
        var returnContent = arrayContent ?? objectContent ?? stringContent;
        
        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            return new BadRequestObjectResult(returnContent);
        }

        _logger.LogInformation("Mail message sent successfully: {@result}", returnContent);
        return new JsonResult(returnContent);
    }
    
    [Function("GetMail")]
    [OpenApiOperation(operationId: "GetMail/{messageId}")]
    [OpenApiSecurity("Authentication", SecuritySchemeType.ApiKey, Name = "X-Functions-Key", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string),
        Description = "The OK response message containing message details")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string),
        Description = "Bad request")]
    public async Task<IActionResult> GetMail([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetMail/{messageId:alpha}")] HttpRequestData req, string messageId)
    {
        _logger.LogInformation("Get mail message status for MessageId {MessageId}", messageId);
        
        (HttpStatusCode statusCode, string result) = await _mailSender.GetMailStatus(messageId);
        if (string.IsNullOrEmpty(result))
        {
            return new BadRequestObjectResult("No content returned from mail service");
        }
        
        (JsonArray? arrayContent, JsonNode? objectContent, string? stringContent) = GetReturnContent(result);
        var returnContent = arrayContent ?? objectContent ?? stringContent;
        
        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            return new BadRequestObjectResult(returnContent);
        }
        
        return new JsonResult(returnContent);
    }

    private static (JsonArray? arrayContent, JsonNode? objectContent, string? stringContent) GetReturnContent(string content)
    {
        try
        {
            JsonNode? nodeContent = JsonNode.Parse(content);

            return nodeContent switch
            {
                JsonArray arrayContent => (arrayContent, null, null),
                JsonObject objectContent => (null, objectContent, null),
                _ => (null, null, content)
            };
        }
        catch (JsonException)
        {
            return (null, null, content);
        }
    }
}