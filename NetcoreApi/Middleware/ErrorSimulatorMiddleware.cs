namespace NetcoreApi.Middleware
{
    public class ErrorSimulatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorSimulatorMiddleware> _logger;
        private static readonly Random _random = new Random();

        public ErrorSimulatorMiddleware(
            RequestDelegate next,
            ILogger<ErrorSimulatorMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check for error simulation header
            var simulateError = context.Request.Headers["X-Simulate-Error"].FirstOrDefault();
            var errorRate = int.Parse(
                context.Request.Headers["X-Error-Rate"].FirstOrDefault() ?? "0");

            if (!string.IsNullOrEmpty(simulateError))
            {
                _logger.LogWarning("Simulating error: {ErrorType}", simulateError);

                switch (simulateError.ToLower())
                {
                    case "500":
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Simulated server error",
                            statusCode = 500,
                            success = false
                        });
                        return;

                    case "timeout":
                        await Task.Delay(60000); // Simulate timeout
                        return;

                    case "network":
                        context.Abort(); // Simulate network disconnection
                        return;

                    case "unauthorized":
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Unauthorized",
                            statusCode = 401,
                            success = false
                        });
                        return;
                }
            }

            // Random error simulation based on error rate
            if (errorRate > 0 && _random.Next(100) < errorRate)
            {
                _logger.LogWarning("Random error triggered (rate: {ErrorRate}%)", errorRate);

                var errorTypes = new[] { 500, 503, 408 };
                var randomError = errorTypes[_random.Next(errorTypes.Length)];

                context.Response.StatusCode = randomError;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = $"Random simulated error (HTTP {randomError})",
                    statusCode = randomError,
                    success = false
                });
                return;
            }

            await _next(context);
        }
    }
}
