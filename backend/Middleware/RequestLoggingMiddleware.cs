using System.Diagnostics;

namespace AlphaOfferService.Middleware
{
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
            if (!_logger.IsEnabled(LogLevel.Debug))
            {
                await _next(context);
                return;
            }

            var requestId = Guid.NewGuid().ToString();
            context.Items["RequestId"] = requestId;

            var stopwatch = Stopwatch.StartNew();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} started. RequestId: {RequestId}, RemoteIP: {RemoteIP}",
                    context.Request.Method,
                    context.Request.Path,
                    requestId,
                    context.Connection.RemoteIpAddress
                );
            }

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var logLevel = GetLogLevelByStatusCode(context.Response.StatusCode);
                if (_logger.IsEnabled(logLevel))
                {
                    _logger.Log(
                        logLevel,
                        "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms. RequestId: {RequestId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        stopwatch.ElapsedMilliseconds,
                        requestId
                    );
                }
            }
        }

        private static LogLevel GetLogLevelByStatusCode(int statusCode)
        {
            if (statusCode >= 500)
                return LogLevel.Error;
            if (statusCode >= 400)
                return LogLevel.Warning;
            return LogLevel.Information;
        }
    }
}
