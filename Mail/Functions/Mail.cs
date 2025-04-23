using System.Net;
using System.Threading.Tasks;
using Mail.Contracts;
using Mail.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    //[ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: "SendMail")]
    [OpenApiSecurity("Authentication", SecuritySchemeType.ApiKey, Name = "X-Functions-Key", In = OpenApiSecurityLocationType.Header)]
    [OpenApiRequestBody("application/json", typeof(Message),
        Description = "JSON request body containing message details.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string),
        Description = "The OK response message containing a JSON result.")]
    public async Task<IActionResult> SendMail([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
        [FromBody] Message message)
    {
        _logger.LogInformation("Retrieved mail message: {@message}", message);
        
        bool result = await _mailSender.SendMail(message);
        
        return new OkObjectResult($"Mail sent: {result}");
    }
}