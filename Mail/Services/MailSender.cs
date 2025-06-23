using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Mail.Services;

public interface IMailSender
{
    Task<(HttpStatusCode, string)> GetMailStatus(string messageId);
    Task<(HttpStatusCode, string)> SendRequest<T>(T message);
}

public class MailSender : IMailSender
{
    private readonly ILogger<MailSender> _logger;
    
    private readonly string _apiAccessToken;
    private readonly HttpClient _httpClient;
    
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public MailSender(IConfiguration config, ILogger<MailSender> logger)
    {
        _logger = logger;

        var apiBaseUrl = config["API_BaseUrl"] ?? throw new InvalidOperationException("API_BaseUrl is missing in configuration");
        _apiAccessToken = config["API_AccessToken"] ?? throw new InvalidOperationException("API_AccessToken is missing in configuration");

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl)
        };
    }

    public async Task<(HttpStatusCode, string)> GetMailStatus(string messageId)
    {
        var url = GenerateUri($"events/messageid/{messageId}");

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return (HttpStatusCode.OK, content);
            }
            
            _logger.LogError(
                "Failed to get mail status for MessageId {MessageId}. Status code: {StatusCode}. Reason: {Reason}. Content: {Content}",
                messageId, response.StatusCode, response.ReasonPhrase, content);
            return (response.StatusCode, content);

        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error getting mail status for MessageId {MessageId}", messageId);
            return (HttpStatusCode.InternalServerError, "Error getting mail status. Please try again later");
        }
    }
    
    public async Task<(HttpStatusCode, string)> SendRequest<T>(T message)
    {
        var messageJson = JsonSerializer.Serialize(message, _options);
        var payload = new StringContent(messageJson, Encoding.UTF8, "application/json");
        var url = GenerateUri("send");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, payload);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("{Type} sent successfully: {@Result}", typeof(T).Name, content);
                return (HttpStatusCode.OK, content);
            }
            
            _logger.LogError(
                "Failed to send mail message of type {Type}. Status code: {StatusCode}. Reason: {Reason}. Content: {Content}. Message: {@Message}",
                typeof(T).Name, response.StatusCode, response.ReasonPhrase, content, message);
            return (response.StatusCode, content);

        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error sending mail message of type {Type}: {@message}", typeof(T).Name, message);
            return (HttpStatusCode.InternalServerError, "Error sending mail message. Please try again later");
        }
    }
    
    private string GenerateUri(string endpoint) => $"{endpoint}?access_token={_apiAccessToken}";
}