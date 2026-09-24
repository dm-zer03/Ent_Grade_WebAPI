using System.Diagnostics;

namespace EcomAPI.Middleware;

public class RequestLoggingMiddleware
{
    private const string RequestIdHeader = "X-Request-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = GetRequestId(context);

        // Make RequestId available throughout the current request.
        context.Items[RequestIdHeader] = requestId;

        // Return the same RequestId to the client.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[RequestIdHeader] = requestId;

            return Task.CompletedTask;
        });

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms. RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId);
        }
    }

    private static string GetRequestId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(
            RequestIdHeader,
            out var requestId)
            && !string.IsNullOrWhiteSpace(requestId))
        {
            return requestId.ToString();
        }

        return context.TraceIdentifier;
    }
}

