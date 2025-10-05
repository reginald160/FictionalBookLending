using FictionalBookLending.src.Application.Contracts;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace FictionalBookLending.src.Application.Middlewares
{
    public sealed class ExceptionHandlingMiddleware
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
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", traceId);
                await HandleExceptionAsync(context, ex, traceId);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, string traceId)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ApiResponse
            {
                Success = false,
                Message = "An unexpected error occurred.",
                Errors = new[] { "your reuqest could not be completed at the momment" },
                TraceId = traceId
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}
