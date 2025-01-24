using Microsoft.AspNetCore.Http;
using Serilog;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();

        await _next(context);

        timer.Stop();
        var duration = timer.ElapsedMilliseconds;

        _logger.LogInformation(
            "Request processing {Method} {Url} completed in {Duration} ms",
            context.Request.Method,
            context.Request.Path,
            duration
        );
    }
}