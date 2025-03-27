using System.Threading.Tasks;
using Mail.Contracts;
using Mail.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    [Function("Mail")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMail([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
        [FromBody] Message message)
    {
        _logger.LogInformation("Retrieved mail message: {@message}", message);
        
        bool result = await _mailSender.SendMail(message);
        
        return new OkObjectResult($"Mail sent: {result}");
    }
}