namespace InventoryManagementAPI.Api.Middleware;

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
        _logger.LogInformation("[HTTP] {Method} {Path}", context.Request.Method, context.Request.Path);

        var startTime = DateTime.UtcNow;
        await _next(context);
        var duration = DateTime.UtcNow - startTime;

        _logger.LogInformation("[HTTP] {Method} {Path} - {StatusCode} ({Duration}ms)", 
            context.Request.Method, context.Request.Path, context.Response.StatusCode, duration.TotalMilliseconds);
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
