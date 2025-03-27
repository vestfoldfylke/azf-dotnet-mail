using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Mail.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Mail.Services;

public class MailSender : IMailSender
{
    private readonly ILogger<MailSender> _logger;

    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly NetworkCredential _smtpCredentials;
    
    public MailSender(IConfiguration config, ILogger<MailSender> logger)
    {
        _logger = logger;
        
        _smtpServer = config["Smtp_Server"] ?? throw new InvalidOperationException("Smtp_Server is missing in configuration");
        _smtpPort = int.TryParse(
            config["Smtp_Port"] ?? throw new InvalidOperationException("Smtp_Port is missing in configuration"),
            out var port)
            ? port
            : 588;
        var smtpUsername = config["Smtp_Username"] ?? throw new InvalidOperationException("Smtp_Username is missing in configuration");
        var smtpPassword = config["Smtp_Password"] ?? throw new InvalidOperationException("Smtp_Password is missing in configuration");
        
        _smtpCredentials = new NetworkCredential(smtpUsername, smtpPassword);
    }
    
    public async Task<bool> SendMail(Message message)
    {
        try
        {
            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort);
            smtpClient.Credentials = _smtpCredentials;
            smtpClient.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(message.From),
                Subject = message.Subject,
                Body = message.Body
            };

            foreach (var recipient in message.Recipients)
            {
                mailMessage.To.Add(new MailAddress(recipient));
            }
            
            await smtpClient.SendMailAsync(mailMessage);

            return true;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogError(ex, "Failed to initialize smtp client with Server {Server} and Port {Port}", _smtpServer, _smtpPort);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "Failed to authenticate with Server {Server} and Username: {Username}", _smtpServer, _smtpCredentials.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send mail message with Server {Server} and Username: {Username}. Message: {@message}", _smtpServer, _smtpCredentials.UserName, message);
        }
        
        return false;
    }
}