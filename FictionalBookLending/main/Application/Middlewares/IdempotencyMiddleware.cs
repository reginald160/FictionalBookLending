using FictionalBookLending.src.Infrastructure.Persistence.Repository;
using System.Text.Json;
using System.Text;
using FictionalBookLending.src.Application.Abstractions;

namespace FictionalBookLending.src.Application.Middlewares
{
    public sealed class IdempotencyMiddleware 
    {
        private readonly RequestDelegate next;
        private readonly ILogger<IdempotencyMiddleware> _logger;

        public IdempotencyMiddleware(RequestDelegate _next, ILogger<IdempotencyMiddleware> logger)
        {
            next = _next;
           _logger = logger;
        }

        
        public async Task InvokeAsync(HttpContext context, IIdempotencyService _idempotencyRepo)
        {
            var method = context.Request.Method.ToUpperInvariant();

            // Only apply to mutating operations
            if (method is not ("POST" or "PUT" or "PATCH"))
            {
                await next(context);
                return;
            }

            // Ensure Idempotency-Key header is present
            if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key) || string.IsNullOrWhiteSpace(key))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Missing required header: Idempotency-Key"
                });
                return;
            }

            var idemKey = key.ToString();

            // Check if we’ve already processed this request
            if (await _idempotencyRepo.ExistsAsync(idemKey, context.RequestAborted))
            {
                _logger.LogInformation("Duplicate request detected for key {Key}", idemKey);

                var cached = await _idempotencyRepo.GetResponseAsync<object>(idemKey, context.RequestAborted);
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(cached));
                return;
            }

            // Capture response body
            var originalBody = context.Response.Body;
            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            // Continue down the pipeline
            await next(context);

            // Read response content
            memStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memStream, Encoding.UTF8).ReadToEndAsync();
            memStream.Seek(0, SeekOrigin.Begin);

            // Clone the response to the client
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            // Cache only successful responses
            if (context.Response.StatusCode is >= 200 and < 300)
            {
                try
                {
                    var json = JsonSerializer.Deserialize<object>(responseBody);
                    await _idempotencyRepo.SaveAsync(idemKey, json!, context.RequestAborted);
                    _logger.LogInformation("Idempotent response cached for key {Key}", idemKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cache response for key {Key}", idemKey);
                }
            }
        }
    }
}
