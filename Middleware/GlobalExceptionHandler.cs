
using EcomAPI.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EcomAPI.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var requestId = httpContext.TraceIdentifier;
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path;

        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,

            ConflictException => StatusCodes.Status409Conflict,

            UnauthorizedException => StatusCodes.Status401Unauthorized,

            ForbiddenException => StatusCodes.Status403Forbidden,

            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. " +
                "Method: {Method}, Path: {Path}, RequestId: {RequestId}",
                method,
                path,
                requestId);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status {StatusCode}. " +
                "Method: {Method}, Path: {Path}, RequestId: {RequestId}",
                statusCode,
                method,
                path,
                requestId);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),

            // Do not expose internal exception details for 500 errors.
            Detail = statusCode >= 500
                ? "An unexpected error occurred."
                : exception.Message,

            Instance = path
        };

        problemDetails.Extensions["requestId"] = requestId;

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status404NotFound =>
                "Resource Not Found",

            StatusCodes.Status409Conflict =>
                "Conflict",

            StatusCodes.Status401Unauthorized =>
                "Unauthorized",

            StatusCodes.Status403Forbidden =>
                "Forbidden",

            _ =>
                "An unexpected error occurred."
        };
    }
}

