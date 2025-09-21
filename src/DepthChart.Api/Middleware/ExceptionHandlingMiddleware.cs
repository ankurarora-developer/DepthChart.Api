using DepthChart.Common.Interfaces;
using System.Text.Json;

namespace DepthChart.Api.Middleware;

internal sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ICorrelationLogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(ICorrelationLogger<ExceptionHandlingMiddleware> logger) => _logger = logger;
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var existingId)
            ? existingId.ToString()
            : Guid.NewGuid().ToString();

            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            await next(context);
        }
        catch (Exception e)
        {
            _logger.Error(e.Message, e);
            await HandleExceptionAsync(context, e);
        }
    }
    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = exception switch
        {
            InvalidOperationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var response = new { error = exception.Message };

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
