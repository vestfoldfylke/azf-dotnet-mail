using System.Collections.Generic;
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

    [Function("send")]
    [OpenApiOperation(operationId: "send")]
    [OpenApiSecurity("Authentication", SecuritySchemeType.ApiKey, Name = "X-Functions-Key", In = OpenApiSecurityLocationType.Header)]
    [OpenApiRequestBody("application/json", typeof(Message),
        Description = "JSON request body containing message details.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(string[]),
        Description = "Input is not valid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string),
        Description = "Bad request (from mail provider)")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dictionary<string, string>),
        Description = "Message(s) sent successfully")]
    public async Task<IActionResult> SendMail([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
        [FromBody] Message message)
    {
        _logger.LogInformation("Sending mail message");
        
        MessageValidator validator = new();
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid)
        {
            return new UnprocessableEntityObjectResult(validationResult.Errors.Select(failure => failure.ErrorMessage));
        }

        var smtPeterMessage = message.GenerateSmtPeterMessage();
        (HttpStatusCode statusCode, string result) = await _mailSender.SendRequest(smtPeterMessage);
        if (string.IsNullOrEmpty(result))
        {
            return new BadRequestObjectResult("No content returned from mail service");
        }
        
        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            return new BadRequestObjectResult(result);
        }

        var returnContent = JsonNode.Parse(result);

        _logger.LogInformation("Mail message sent successfully: {@result}", result);
        return new JsonResult(returnContent);
    }
    
    [Function("bulksend")]
    [OpenApiOperation(operationId: "bulksend")]
    [OpenApiSecurity("Authentication", SecuritySchemeType.ApiKey, Name = "X-Functions-Key", In = OpenApiSecurityLocationType.Header)]
    [OpenApiRequestBody("application/json", typeof(BulkMessage),
        Description = "JSON request body containing message details.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(string[]),
        Description = "Input is not valid")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string),
        Description = "Bad request (from mail provider)")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dictionary<string, string>),
        Description = "Message(s) sent successfully")]
    public async Task<IActionResult> BulkSendMail([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
        [FromBody] BulkMessage message)
    {
        _logger.LogInformation("Sending bulk mail message");
        
        BulkMessageValidator validator = new();
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid)
        {
            return new UnprocessableEntityObjectResult(validationResult.Errors.Select(failure => failure.ErrorMessage));
        }
        
        var smtPeterBulkMessage = message.GenerateSmtPeterBulkMessage();
        (HttpStatusCode statusCode, string result) = await _mailSender.SendRequest(smtPeterBulkMessage);
        if (string.IsNullOrEmpty(result))
        {
            return new BadRequestObjectResult("No content returned from mail service");
        }
        
        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            return new BadRequestObjectResult(result);
        }

        var returnContent = JsonNode.Parse(result);

        _logger.LogInformation("Mail message sent successfully: {@result}", result);
        return new JsonResult(returnContent);
    }
    
    [Function("status")]
    [OpenApiOperation(operationId: "status/{messageId}")]
    [OpenApiSecurity("Authentication", SecuritySchemeType.ApiKey, Name = "X-Functions-Key", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string),
        Description = "Bad request (from mail provider)")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<EventMessage>),
        Description = "Message details")]
    public async Task<IActionResult> GetStatus([HttpTrigger(AuthorizationLevel.Function, "get", Route = "status/{messageId:alpha}")] HttpRequestData req, string messageId)
    {
        _logger.LogInformation("Get mail message status for MessageId {MessageId}", messageId);
        
        (HttpStatusCode statusCode, string result) = await _mailSender.GetMailStatus(messageId);
        if (string.IsNullOrEmpty(result))
        {
            return new BadRequestObjectResult("No content returned from mail service");
        }
        
        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            return new BadRequestObjectResult(result);
        }

        var returnContent = JsonSerializer.Deserialize<IEnumerable<EventMessage>>(result);
        
        return new JsonResult(returnContent);
    }
}