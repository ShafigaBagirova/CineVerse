using Application.Common.Responses;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var requestPath = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;
            var traceId = context.TraceIdentifier;

            var errorsSummary = string.Join(" | ",
                ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

            _logger.LogWarning(
                ex,
                "Validation failed. Method: {Method}, RequestPath: {RequestPath}, TraceId: {TraceId}, Errors: {Errors}",
                method,
                requestPath,
                traceId,
                errorsSummary);

            await WriteValidationResponseAsync(context, ex, traceId);
        }
        catch (Exception ex)
        {
            var requestPath = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;
            var traceId = context.TraceIdentifier;

            _logger.LogError(
                ex,
                "Unhandled exception. Method: {Method}, RequestPath: {RequestPath}, TraceId: {TraceId}, ExceptionType: {ExceptionType}",
                method,
                requestPath,
                traceId,
                ex.GetType().Name);

            await WriteErrorResponseAsync(context, traceId);
        }
    }

    private static async Task WriteValidationResponseAsync(
        HttpContext context,
        ValidationException ex,
        string traceId)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var errors = ex.Errors
       .Select(e =>
        $"{e.PropertyName}: {e.ErrorMessage}")
        .ToList();
        var body = new BaseResponse<object>
        {
            Success = false,
            Message = "Validation failed.",
            Errors = errors,
            TraceId = traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        string traceId)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var body = new BaseResponse<object>
        {
            Success = false,
            Message = "An unexpected error occurred.",
            Errors = null,
            TraceId = traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
