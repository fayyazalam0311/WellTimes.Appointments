using AuthService.Exceptions;

namespace AuthService.Middleware
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
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Handled application error: {Message}", ex.Message);
                await WriteResponseAsync(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occured.");
                await WriteResponseAsync(context, StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteResponseAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new { status = statusCode, message });
        }
    }
}
