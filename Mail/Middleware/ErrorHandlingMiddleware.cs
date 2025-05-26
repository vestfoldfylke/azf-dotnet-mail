using System;
using System.Text.Json;
using System.Threading.Tasks;
using Mail.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Mail.Middleware;

public class ErrorHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }
    
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        // NOTE: Add more specific exception handling as needed to minimize data exposure
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            
            await HandleException(context, "An unhandled exception occurred", ex.Message);
        }
    }

    private static async Task HandleException(FunctionContext context, string message, string? exceptionMessage)
    {
        var request = await context.GetHttpRequestDataAsync();
        var response = request!.CreateResponse();
        response.StatusCode = System.Net.HttpStatusCode.InternalServerError;

        var errorMessage = new ErrorResponse(message, exceptionMessage);
        var jsonResponse = JsonSerializer.Serialize(errorMessage);
            
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(jsonResponse);
            
        context.GetInvocationResult().Value = response;
    }
}