using Application.Common.Responses;
using FluentValidation;
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
            if (context.Response.HasStarted)
                throw;

            _logger.LogWarning(
                ex,
                "Validation failed. Path: {Path}",
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
           .Select(e => e.ErrorMessage)
           .Distinct()
           .ToList();

            var response = BaseResponse.Fail(
                "Validation failed",
                errors);

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
                throw;

            _logger.LogError(
                ex,
                "Unhandled exception. Path: {Path}",
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = BaseResponse.Fail("Server error");

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
