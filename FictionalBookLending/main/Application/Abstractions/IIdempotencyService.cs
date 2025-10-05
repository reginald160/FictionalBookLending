namespace FictionalBookLending.src.Application.Abstractions
{
    public interface IIdempotencyService
    {
        Task<bool> ExistsAsync(string key, CancellationToken ct);
        Task SaveAsync(string key, object response, CancellationToken ct);
        Task<T?> GetResponseAsync<T>(string key, CancellationToken ct);
    }
}
