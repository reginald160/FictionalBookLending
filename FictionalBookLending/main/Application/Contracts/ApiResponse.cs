namespace FictionalBookLending.src.Application.Contracts
{

    /// <summary>
    /// Generic API response wrapper compatible with Minimal API results.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; init; }
        public string? TraceId { get; init; }
        public object Data { get; init; }

        public static IResult Ok(object data = null, string? message = null, string? traceId = null)
            => Results.Ok(new ApiResponse
            {
                Success = true,
                Message = message ?? "Operation successful",
                TraceId = traceId,
                Data = data
            });

        public static IResult Created(string location, object data = default, string? message = null, string? traceId = null)
            => Results.Created(location, new ApiResponse
            {
                Success = true,
                Message = message ?? "Resource created successfully",
                TraceId = traceId,
                Data = data
            });

        public static IResult Fail(IEnumerable<string> errors, string? message = null, string? traceId = null)
            => Results.BadRequest(new ApiResponse
            {
                Success = false,
                Message = message ?? "Operation failed",
                Errors = errors,
                TraceId = traceId
            });

        public static IResult NotFound(string? message = null, string? traceId = null)
            => Results.NotFound(new ApiResponse
            {
                Success = false,
                Message = message ?? "Resource not found",
                TraceId = traceId
            });

       
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; init; }
        public string? TraceId { get; init; }
        public T? Data { get; init; }

        public static IResult Ok(T ? data = default, string? message = null, string? traceId = null)
            => Results.Ok(new ApiResponse
            {
                Success = true,
                Message = message ?? "Operation successful",
                TraceId = traceId,
                Data = data
            });

 


    }

}
