using System.Diagnostics;
using System.Text.Json;
using Evaluation.Common;

namespace Evaluation.API.Helper;

public class Middleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<Middleware> _logger;

    public Middleware(RequestDelegate next, ILogger<Middleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Request: {Method} {Path} at {Time}", context.Request.Method, context.Request.Path, DateTime.UtcNow);
            await _next(context);
        }
        catch (HttpStatusCodeException httpEx)
        {
            _logger.LogWarning(httpEx, "HttpStatusCodeException thrown for {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = httpEx.StatusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                StatusCode = httpEx.StatusCode,
                Message = httpEx.Message,
                Code = httpEx.Code,
                Data = httpEx.Data
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing request: {Method} {Path} at {Time}", context.Request.Method, context.Request.Path, DateTime.UtcNow);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var response = new
            {
                StatusCode = 500,
                Message = "An unexpected error occurred."
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Finished: {Method} {Path} in {ElapsedMilliseconds}ms", context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
        }
    }
}
