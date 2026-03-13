using System.Diagnostics;

namespace API.Middlewares;

public class RequestLoggingMiddleware
{

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;
        var traceId = context.TraceIdentifier;
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        _logger.LogInformation(
         "Request started: {Method} {Path}, TraceId: {TraceId}, IP: {IP}, UserAgent: {UserAgent}",
         method, path, traceId, ip, userAgent);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var durationMs = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                _logger.LogError(
                    "Request completed: {Method} {Path}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                    method, path, statusCode, durationMs);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Request completed: {Method} {Path}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                    method, path, statusCode, durationMs);
            }
            else
            {
                _logger.LogInformation(
                    "Request completed: {Method} {Path}, StatusCode: {StatusCode}, DurationMs: {DurationMs}",
                    method, path, statusCode, durationMs);
            }
        }
    }
}
