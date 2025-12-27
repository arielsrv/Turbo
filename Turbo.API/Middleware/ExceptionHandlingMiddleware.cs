using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Turbo.API.Exceptions;

namespace Turbo.API.Middleware;

/// <summary>
///     Middleware that catches exceptions and converts them to RFC 7807 Problem Details responses.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(context, exception);

        LogException(exception, problemDetails);

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null,
            "application/problem+json");
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails(validationEx.Errors)
            {
                Type = validationEx.Type,
                Title = validationEx.Title,
                Status = validationEx.StatusCode,
                Detail = validationEx.Message,
                Instance = context.Request.Path
            },
            DomainException domainEx => new ProblemDetails
            {
                Type = domainEx.Type,
                Title = domainEx.Title,
                Status = domainEx.StatusCode,
                Detail = domainEx.Message,
                Instance = context.Request.Path
            },
            // Handle legacy InvalidOperationException as a business error
            InvalidOperationException invalidOpEx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = invalidOpEx.Message,
                Instance = context.Request.Path
            },
            // Handle all other exceptions as internal server errors
            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                Instance = context.Request.Path
            }
        };
    }

    private void LogException(Exception exception, ProblemDetails problemDetails)
    {
        if (problemDetails.Status >= 500)
            logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        else
            logger.LogWarning("A handled exception occurred: {Title} - {Detail}",
                problemDetails.Title, problemDetails.Detail);
    }
}

/// <summary>
///     Extension methods for registering the ExceptionHandlingMiddleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static void UseExceptionHandling(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}