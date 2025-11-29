namespace AlphaOfferService.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    var requestId = context.Items["RequestId"]?.ToString() ?? "Unknown";

                    _logger.LogError(
                        e,
                        "Unhandled exception occurred. RequestId: {RequestId}, Path: {Path}, Method: {Method}",
                        requestId,
                        context.Request.Path,
                        context.Request.Method
                    );
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "An error occurred while processing your request.",
                    requestId = context.Items["RequestId"]?.ToString() ?? "Unknown",
                    timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
