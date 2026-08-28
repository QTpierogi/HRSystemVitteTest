using System.Text.Json;

namespace HRSystem.Api.Middleware
{
    public record ExceptionResponse(string Message, string TraceId);

    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;

                _logger.LogError(ex,
                    "Необработанное исключение при обработке запроса {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, traceId);

                await HandleExceptionAsync(context, traceId);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, string traceId)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new ExceptionResponse(
                Message: "Произошла внутренняя ошибка сервера. Обратитесь к администратору, указав TraceId.",
                TraceId: traceId);

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
