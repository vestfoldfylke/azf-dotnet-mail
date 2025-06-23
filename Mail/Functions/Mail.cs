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
using Vestfold.Extensions.Metrics.Services;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace Mail.Functions;

public class Mail
{
    private readonly ILogger<Mail> _logger;
    private readonly IMailSender _mailSender;
    private readonly IMetricsService _metricsService;

    private readonly JsonSerializerOptions _camelCaseOptions = new JsonSerializerOptions
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public Mail(ILogger<Mail> logger, IMailSender mailSender, IMetricsService metricsService)
    {
        _logger = logger;
        _mailSender = mailSender;
        _metricsService = metricsService;
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
            _logger.LogWarning("Validation failed for message. Errors: {@Errors}", validationResult.Errors);
            _metricsService.Count($"{Constants.MetricsPrefix}_Send", "Email requests through SMTP API", ("Result", "ValidationFailed"));
            return new UnprocessableEntityObjectResult(validationResult.Errors.Select(failure => failure.ErrorMessage));
        }

        var smtPeterMessage = message.GenerateSmtPeterMessage();
        var (statusCode, result) = await _mailSender.SendRequest(smtPeterMessage);
        if (string.IsNullOrEmpty(result))
        {
            _logger.LogError("No content returned from mail service");
            _metricsService.Count($"{Constants.MetricsPrefix}_Send", "Email requests through SMTP API", ("Result", "Failed"));
            return new BadRequestObjectResult("No content returned from mail service");
        }
        
        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            _metricsService.Count($"{Constants.MetricsPrefix}_Send", "Email requests through SMTP API", ("Result", "Failed"));
            return new BadRequestObjectResult(result);
        }

        var jsonResult = JsonNode.Parse(result);
        if (jsonResult is null)
        {
            _metricsService.Count($"{Constants.MetricsPrefix}_Send", "Email requests through SMTP API", ("Result", "Failed"));
            return new BadRequestObjectResult("Failed to parse response from mail service");
        }
        
        _metricsService.Count($"{Constants.MetricsPrefix}_Send", "Email requests through SMTP API", ("Result", "Success"));
        _metricsService.Count($"{Constants.MetricsPrefix}_Send_Recipients", "Email sent through SMTP API", jsonResult.AsObject().Count);
        return new JsonResult(JsonNode.Parse(result));
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
            _logger.LogWarning("Validation failed for bulk message. Errors: {@Errors}", validationResult.Errors);
            _metricsService.Count($"{Constants.MetricsPrefix}_BulkSend", "Bulk email requests through SMTP API", ("Result", "ValidationFailed"));
            return new UnprocessableEntityObjectResult(validationResult.Errors.Select(failure => failure.ErrorMessage));
        }
        
        var smtPeterBulkMessage = message.GenerateSmtPeterBulkMessage();
        var (statusCode, result) = await _mailSender.SendRequest(smtPeterBulkMessage);
        if (string.IsNullOrEmpty(result))
        {
            _logger.LogError("No content returned from mail service");
            _metricsService.Count($"{Constants.MetricsPrefix}_BulkSend", "Bulk email requests through SMTP API", ("Result", "Failed"));
            return new BadRequestObjectResult("No content returned from mail service");
        }
        
        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            _metricsService.Count($"{Constants.MetricsPrefix}_BulkSend", "Bulk email requests through SMTP API", ("Result", "Failed"));
            return new BadRequestObjectResult(result);
        }

        var jsonResult = JsonNode.Parse(result);
        if (jsonResult is null)
        {
            _metricsService.Count($"{Constants.MetricsPrefix}_BulkSend", "Bulk email requests through SMTP API", ("Result", "Failed"));
            return new BadRequestObjectResult("Failed to parse response from mail service");
        }
        
        _metricsService.Count($"{Constants.MetricsPrefix}_BulkSend", "Bulk email requests through SMTP API", ("Result", "Success"));
        _metricsService.Count($"{Constants.MetricsPrefix}_BulkSend_Recipients", "Email sent through SMTP API", jsonResult.AsObject().Count);
        return new JsonResult(jsonResult);
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
        
        var (statusCode, result) = await _mailSender.GetMailStatus(messageId);
        if (string.IsNullOrEmpty(result))
        {
            return new BadRequestObjectResult("No content returned from mail service");
        }
        
        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            return new BadRequestObjectResult(result);
        }

        var returnContent = JsonSerializer.Deserialize<IEnumerable<EventMessage>>(result, _camelCaseOptions);
        
        return new JsonResult(returnContent);
    }
}