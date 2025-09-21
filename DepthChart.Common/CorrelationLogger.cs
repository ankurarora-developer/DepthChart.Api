using DepthChart.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DepthChart.Common;
public class CorrelationLogger<T>(ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor) : ICorrelationLogger<T>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(typeof(T));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public void Info(string message)
    {
        _logger.LogInformation(FormatCorrelationMessage(message));
    }

    public void Info(string message, object eventObject)
    {
        _logger.LogInformation(FormatCorrelationMessage(message, eventObject));
    }

    public void Debug(string message)
    {
        _logger.LogDebug(FormatCorrelationMessage(message));
    }

    public void Debug(string message, object eventObject)
    {
        _logger.LogDebug(FormatCorrelationMessage(message, eventObject));
    }

    public void Error(string message)
    {
        _logger.LogError(FormatCorrelationMessage(message));
    }

    public void Error(string message, object eventObject)
    {
        _logger.LogError(FormatCorrelationMessage(message, eventObject));
    }

    private string FormatCorrelationMessage(string eventMessage, object eventObject)
    {
        string correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? "N/A";
        var eventDescription = (!string.IsNullOrEmpty(eventMessage) ? $" | [{eventMessage}]" : string.Empty);
        return $"[{correlationId}]{eventDescription} | {eventObject}";
    }

    private string FormatCorrelationMessage(string message)
    {
        string correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? "N/A";
        return $"[{correlationId}]{message}";
    }
}
