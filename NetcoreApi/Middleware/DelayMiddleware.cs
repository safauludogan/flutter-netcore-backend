namespace NetcoreApi.Middleware
{
    public class DelayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DelayMiddleware> _logger;

        public DelayMiddleware(RequestDelegate next, ILogger<DelayMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check for delay header
            var delayMs = int.Parse(
                context.Request.Headers["X-Delay-Ms"].FirstOrDefault() ?? "0");

            if (delayMs > 0)
            {
                _logger.LogInformation("Adding delay: {DelayMs}ms", delayMs);
                await Task.Delay(delayMs);
            }

            await _next(context);
        }
    }
}
